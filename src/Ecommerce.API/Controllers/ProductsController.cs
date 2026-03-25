using AutoMapper;
using Ecommerce.API.Models.Products;
using Ecommerce.Application.Common.Interfaces;
using AppServices = Ecommerce.Application.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(IProductService productService, IMapper mapper) : ControllerBase
{
    /// <summary>Gets all products.</summary>
    /// <response code="200">Returns the list of products.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);
        return Ok(mapper.Map<IEnumerable<ProductResponse>>(products));
    }

    /// <summary>Gets a product by ID.</summary>
    /// <response code="200">Returns the product.</response>
    /// <response code="404">Product not found.</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return Ok(mapper.Map<ProductResponse>(product));
    }

    /// <summary>Creates a new product.</summary>
    /// <response code="201">Returns the newly created product ID.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden – Admin role required.</response>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var serviceRequest = mapper.Map<AppServices.CreateProductRequest>(request);
        var id = await productService.CreateAsync(serviceRequest, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Updates an existing product.</summary>
    /// <response code="204">Product updated successfully.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden – Admin role required.</response>
    /// <response code="404">Product not found.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var serviceRequest = mapper.Map<AppServices.UpdateProductRequest>(request);
        await productService.UpdateAsync(id, serviceRequest, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a product by ID.</summary>
    /// <response code="204">Product deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden – Admin role required.</response>
    /// <response code="404">Product not found.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
