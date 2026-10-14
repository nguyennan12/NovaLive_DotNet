using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Categories.Commands.CreateCategory;
using NovaLive.Application.UseCases.Categories.Commands.DeleteCategory;
using NovaLive.Application.UseCases.Categories.Commands.UpdateCategory;
using NovaLive.Application.UseCases.Categories.Queries.GetCategoriesTree;
using NovaLive.Application.UseCases.Categories.Queries.GetCategoryById;
using NovaLive.Contracts.V1.Categories;

namespace NovaLive.Api.Controllers.Categories;

[Route("api/v1/categories")]
[Route("categories")]
public sealed class CategoriesController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>
    /// Lấy toàn bộ cây danh mục đa cấp (Parent-Child hierarchy) kèm số lượng sản phẩm ProductCount.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<IReadOnlyList<CategoryNodeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoriesTree(CancellationToken ct)
    {
        return Respond(await Sender.Send(new GetCategoriesTreeQuery(), ct));
    }

    /// <summary>
    /// Lấy chi tiết thông tin danh mục theo ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken ct)
    {
        return Respond(await Sender.Send(new GetCategoryByIdQuery(id), ct));
    }

    /// <summary>
    /// Tạo mới danh mục (Admin).
    /// </summary>
    [HttpPost]
    [HttpPost("/admin/categories")]
    [HttpPost("/api/v1/admin/categories")]
    [Authorize]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<CategoryResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken ct)
    {
        return Respond(await Sender.Send(new CreateCategoryCommand(request), ct), StatusCodes.Status201Created);
    }

    /// <summary>
    /// Cập nhật thông tin danh mục (Admin).
    /// </summary>
    [HttpPut("{id:guid}")]
    [HttpPut("/admin/categories/{id:guid}")]
    [HttpPut("/api/v1/admin/categories/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken ct)
    {
        return Respond(await Sender.Send(new UpdateCategoryCommand(id, request), ct));
    }

    /// <summary>
    /// Xóa danh mục (Admin). Ràng buộc không có danh mục con hoặc sản phẩm.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HttpDelete("/admin/categories/{id:guid}")]
    [HttpDelete("/api/v1/admin/categories/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken ct)
    {
        return Respond(await Sender.Send(new DeleteCategoryCommand(id), ct));
    }
}
