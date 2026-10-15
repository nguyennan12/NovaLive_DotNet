using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Categories.Queries.GetCategoriesTree;
using NovaLive.Application.UseCases.Categories.Queries.GetCategoryById;
using NovaLive.Contracts.V1.Categories;

namespace NovaLive.Api.Controllers.Categories;

/// <summary>
/// Controller công khai cung cấp các thao tác xem thông tin và cây danh mục sản phẩm đa cấp.
/// </summary>
/// <param name="sender">MediatR sender dùng để gửi truy vấn danh mục.</param>
[AllowAnonymous]
[Route("api/v1/categories")]
[Route("categories")]
public sealed class CategoriesController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>
    /// Lấy toàn bộ cây danh mục đa cấp (Parent-Child hierarchy) kèm số lượng sản phẩm ProductCount.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<IReadOnlyList<CategoryNodeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoriesTree(CancellationToken ct)
    {
        return Respond(await Sender.Send(new GetCategoriesTreeQuery(), ct));
    }

    /// <summary>
    /// Lấy chi tiết thông tin danh mục theo ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken ct)
    {
        return Respond(await Sender.Send(new GetCategoryByIdQuery(id), ct));
    }
}
