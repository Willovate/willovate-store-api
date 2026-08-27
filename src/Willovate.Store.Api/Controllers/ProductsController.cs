using Microsoft.AspNetCore.Mvc;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? featured,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var response = await productService.GetProductsAsync(
            search,
            category,
            featured,
            page,
            pageSize,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("categories")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCategories(
        CancellationToken cancellationToken) =>
        Ok(await productService.GetCategoriesAsync(cancellationToken));

    [HttpGet("{slug}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProduct(
        string slug,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetBySlugAsync(slug, cancellationToken);

        return product is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found")
            : Ok(product);
    }
}
