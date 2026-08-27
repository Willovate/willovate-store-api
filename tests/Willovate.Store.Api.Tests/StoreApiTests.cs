using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Willovate.Store.Api.Contracts;

namespace Willovate.Store.Api.Tests;

public sealed class StoreApiTests : IAsyncLifetime
{
    private WebApplicationFactory<Program>? factory;
    private HttpClient? client;

    public Task InitializeAsync()
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });

        client = factory.CreateClient();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task HealthEndpointReportsAHealthyService()
    {
        var response = await client!.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ProductsEndpointReturnsSeededFeaturedProducts()
    {
        var response = await client!.GetAsync("/api/products?featured=true&pageSize=20");
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, product => Assert.True(product.IsFeatured));
    }

    [Fact]
    public async Task UnknownProductReturnsProblemDetails()
    {
        var response = await client!.GetAsync("/api/products/not-a-real-product");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();

        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

    }
}
