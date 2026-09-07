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

    [Fact]
    public async Task TemplatesEndpointReturnsSeededClothingStoreTemplates()
    {
        var response = await client!.GetAsync("/api/templates?businessType=clothing-store");
        var templates = await response.Content.ReadFromJsonAsync<IReadOnlyList<TemplateResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(templates);
        Assert.NotEmpty(templates);
        Assert.All(templates, t => Assert.Equal("clothing-store", t.BusinessType));
    }

    [Fact]
    public async Task TemplatesEndpointFiltersByTag()
    {
        var response = await client!.GetAsync("/api/templates?businessType=clothing-store&tag=Minimal");
        var templates = await response.Content.ReadFromJsonAsync<IReadOnlyList<TemplateResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(templates);
        Assert.NotEmpty(templates);
        Assert.All(templates, t => Assert.Contains("Minimal", t.Tags));
    }

    [Fact]
    public async Task SelectTemplateSuccessfullySavesSelectionAndReturnsWorkspaceUrl()
    {
        var templatesResponse = await client!.GetAsync("/api/templates?businessType=clothing-store");
        var templates = await templatesResponse.Content.ReadFromJsonAsync<IReadOnlyList<TemplateResponse>>();
        Assert.NotNull(templates);
        var selected = templates[0];

        var payload = new SelectTemplateRequest(
            SessionId: "test_session_123",
            TemplateId: selected.Id,
            IsBlank: false);

        var response = await client.PostAsJsonAsync("/api/onboarding/select-template", payload);
        var result = await response.Content.ReadFromJsonAsync<SelectTemplateResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotEmpty(result.ProjectId);
        Assert.StartsWith("/workspace/", result.NextStepUrl);
    }

    [Fact]
    public async Task SelectTemplateWithBlankCanvasSucceeds()
    {
        var payload = new SelectTemplateRequest(
            SessionId: "test_session_blank",
            TemplateId: null,
            IsBlank: true);

        var response = await client!.PostAsJsonAsync("/api/onboarding/select-template", payload);
        var result = await response.Content.ReadFromJsonAsync<SelectTemplateResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.StartsWith("/workspace/", result.NextStepUrl);
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
