using Microsoft.AspNetCore.Mvc;
using Stockroom.Application.Products;

namespace Stockroom.Api.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    /// <summary>Every product, or only those still sold when <paramref name="activeOnly"/> is set.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> List(
        [FromQuery] bool activeOnly,
        CancellationToken cancellationToken)
    {
        var products = await productService.ListAsync(activeOnly, cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetAsync(id, cancellationToken);
        return Ok(product);
    }

    /// <summary>Creates a product; its SKU is stored upper-case and must be unique.</summary>
    [HttpPost]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.UpdateAsync(id, request, cancellationToken);
        return Ok(product);
    }

    [HttpPut("{id:guid}/reorder-threshold")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> ChangeReorderThreshold(
        Guid id,
        [FromBody] ChangeReorderThresholdRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.ChangeReorderThresholdAsync(id, request, cancellationToken);
        return Ok(product);
    }

    /// <summary>Adds stock on hand; a discontinued product cannot be restocked.</summary>
    [HttpPost("{id:guid}/restock")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductDto>> Restock(
        Guid id,
        [FromBody] RestockRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.RestockAsync(id, request, cancellationToken);
        return Ok(product);
    }

    // Discontinuing keeps the row: past orders still refer to the product by its id.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Discontinue(Guid id, CancellationToken cancellationToken)
    {
        await productService.DiscontinueAsync(id, cancellationToken);
        return NoContent();
    }
}
