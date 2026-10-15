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

[Authorize]
[Route("api/v1/admin/categories")]
[Route("admin/categories")]
public sealed class AdminCategoriesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<CategoryResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken ct)
    {
        return Respond(await Sender.Send(new CreateCategoryCommand(request), ct), StatusCodes.Status201Created);
    }

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
