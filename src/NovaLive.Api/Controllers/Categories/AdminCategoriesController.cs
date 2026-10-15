using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Categories.Commands.CreateCategory;
using NovaLive.Application.UseCases.Categories.Commands.DeleteCategory;
using NovaLive.Application.UseCases.Categories.Commands.UpdateCategory;
using NovaLive.Contracts.V1.Categories;

namespace NovaLive.Api.Controllers.Categories;

/// <summary>
/// Controller dành cho Quản trị viên quản lý danh mục sản phẩm (thêm, cập nhật, xóa).
/// </summary>
/// <param name="sender">MediatR sender dùng để gửi command quản trị danh mục.</param>
[Authorize]
[Route("api/v1/admin/categories")]
[Route("admin/categories")]
public sealed class AdminCategoriesController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>
    /// Tạo mới danh mục (Admin).
    /// </summary>
    [HttpPost]
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
