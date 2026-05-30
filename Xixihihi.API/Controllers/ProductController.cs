using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xixihihi.Application.DTOs.Requests;
using Xixihihi.Application.Features.Products.Commands;
using Xixihihi.Application.Features.Products.Queries;
using Xixihihi.Domain.Enums;

namespace Xixihihi.API.Controllers;

[Route("api/products")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetProductByIdQuery { Id = id }, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var sellerId))
        {
            return Unauthorized(new { Message = "Invalid token claims." });
        }

        var command = new CreateProductCommand
        {
            SellerId = sellerId,
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            IsNegotiable = request.IsNegotiable,
            Condition = request.Condition,
            TransactionType = request.TransactionType,
            CategoryId = request.CategoryId,
            Brand = request.Brand,
            Size = request.Size,
            ProvinceId = request.ProvinceId,
            WardId = request.WardId
        };

        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProductById), new { id = response.Data.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var sellerId))
        {
            return Unauthorized(new { Message = "Invalid token claims." });
        }

        var command = new UpdateProductCommand
        {
            Id = id,
            SellerId = sellerId,
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            IsNegotiable = request.IsNegotiable,
            Condition = request.Condition,
            TransactionType = request.TransactionType,
            CategoryId = request.CategoryId,
            Brand = request.Brand,
            Size = request.Size,
            ProvinceId = request.ProvinceId,
            WardId = request.WardId
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateProductStatus(Guid id, [FromBody] UpdateProductStatusRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var sellerId))
        {
            return Unauthorized(new { Message = "Invalid token claims." });
        }

        var command = new UpdateProductStatusCommand
        {
            Id = id,
            SellerId = sellerId,
            Status = request.Status
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var sellerId))
        {
            return Unauthorized(new { Message = "Invalid token claims." });
        }

        var command = new DeleteProductCommand
        {
            Id = id,
            SellerId = sellerId
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("{id:guid}/images")]
    [Authorize]
    public async Task<IActionResult> UploadProductImage(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Message = "No file uploaded." });
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { Message = $"File '{file.FileName}' vượt quá 5MB." });
        }

        if (!file.ContentType.StartsWith("image/"))
        {
            return BadRequest(new { Message = $"File '{file.FileName}' không phải định dạng ảnh." });
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var sellerId))
        {
            return Unauthorized(new { Message = "Invalid token claims." });
        }

        var command = new UploadProductImageCommand
        {
            ProductId = id,
            SellerId = sellerId,
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{id:guid}/images/{imgId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteProductImage(Guid id, Guid imgId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var sellerId))
        {
            return Unauthorized(new { Message = "Invalid token claims." });
        }

        var command = new DeleteProductImageCommand
        {
            ProductId = id,
            ImageId = imgId,
            SellerId = sellerId
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
