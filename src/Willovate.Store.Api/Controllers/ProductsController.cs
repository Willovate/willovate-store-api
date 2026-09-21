using Microsoft.AspNetCore.Mvc;
using Willovate.Store.Api.Contracts;
using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    private const long MaxImageBytes = 10 * 1024 * 1024; // 10 MB per file
    private static readonly HashSet<string> AllowedExtensions = [
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".avif", ".jfif", ".svg", ".bmp"
    ];

    // ── Public (storefront) ────────────────────────────────────────────────
    // IsActive = true filtering is enforced by the service; nothing changes here.

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
            search, category, featured, page, pageSize, cancellationToken);

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

    // ── Admin ──────────────────────────────────────────────────────────────
    // TODO: Protect with an authorization policy when auth is added.

    // "~/" prefix creates an absolute route, bypassing the api/products class prefix.
    [HttpGet("~/api/admin/products")]
    [ProducesResponseType<PagedResponse<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAdminProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? status,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? productTypes = null,
        [FromQuery] string? tags = null,
        [FromQuery] string? collections = null,
        [FromQuery] string? brands = null,
        [FromQuery] string? brand = null,
        [FromQuery] string? stockStatus = null,
        [FromQuery] string? variantFilter = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? updatedRange = null,
        [FromQuery] DateTimeOffset? updatedFrom = null,
        [FromQuery] DateTimeOffset? updatedTo = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveBrands = !string.IsNullOrWhiteSpace(brands) ? brands : brand;
        var response = await productService.GetAdminProductsAsync(
            search, category, status, sortBy, page, pageSize,
            productTypes, tags, collections, effectiveBrands, stockStatus, variantFilter, minPrice, maxPrice, updatedRange, updatedFrom, updatedTo,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("~/api/admin/products/{id:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetAdminProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);

        return product is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found")
            : Ok(product);
    }

    [HttpPost]

    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetProduct), new { slug = product.Slug }, product);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.UpdateAsync(id, request, cancellationToken);

        return product is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found")
            : Ok(product);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await productService.DeleteAsync(id, cancellationToken);

        return deleted
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found");
    }

    // ── Image management ───────────────────────────────────────────────────

    /// <summary>
    /// GET /api/products/{id}/images to get image URLs for a product.
    /// </summary>
    [HttpGet("{id:guid}/images")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetImages(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return product is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found")
            : Ok(product.ImageUrls);
    }

    /// <summary>
    /// Save new image files to storage. Returns the newly saved relative URLs only —
    /// does NOT update the product record. The caller must follow up with
    /// PUT /api/products/{id}/images to commit the final ordered URL list.
    /// This two-step design lets the frontend merge existing + new images freely.
    /// </summary>
    [HttpPost("{id:guid}/images")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<string>>> UploadImages(
        Guid id,
        List<IFormFile>? files,
        CancellationToken cancellationToken)
    {
        var formFiles = (files != null && files.Count > 0)
            ? files
            : (Request.HasFormContentType ? Request.Form.Files.ToList() : []);

        var errors  = new List<string>();
        var newUrls = new List<string>();

        if (formFiles.Count == 0)
            return Ok(newUrls);

        if (formFiles.Count > 10)
            return BadRequest(new { error = "A maximum of 10 images can be uploaded at once." });

        foreach (var file in formFiles)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                errors.Add($"'{file.FileName}': unsupported format. Allowed formats: PNG, JPG, JPEG, WEBP, GIF, AVIF, SVG.");
                continue;
            }

            if (file.Length > MaxImageBytes)
            {
                errors.Add($"'{file.FileName}': exceeds the 10 MB limit ({file.Length / 1_048_576.0:F1} MB).");
                continue;
            }

            await using var stream = file.OpenReadStream();
            var url = await productService.SaveImageAsync(stream, file.FileName, cancellationToken);
            if (url is not null) newUrls.Add(url);
        }

        if (errors.Count > 0 && newUrls.Count == 0)
            return BadRequest(new { errors });

        return Ok(newUrls);
    }

    /// <summary>
    /// Persist the final ordered list of image URLs for a product.
    /// Replaces the current imageUrls stored on the product.
    /// Call this after uploading to combine existing and new URLs in desired order.
    /// </summary>
    [HttpPut("{id:guid}/images")]
    public async Task<ActionResult<ProductResponse>> UpdateImages(
        Guid id,
        [FromBody] string[]? imageUrls,
        CancellationToken cancellationToken)
    {
        var product = await productService.UpdateImagesAsync(id, imageUrls ?? [], cancellationToken);

        return product is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found")
            : Ok(product);
    }
}
