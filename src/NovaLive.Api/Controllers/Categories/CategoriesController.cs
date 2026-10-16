using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Categories.Queries.GetCategoriesTree;
using NovaLive.Application.UseCases.Categories.Queries.GetCategoryById;
using NovaLive.Contracts.V1.Categories;

namespace NovaLive.Api.Controllers.Categories;

[AllowAnonymous]
[Route("api/v1/categories")]
[Route("categories")]
public sealed class CategoriesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<IReadOnlyList<CategoryNodeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoriesTree(CancellationToken ct)
    {
        return Respond(await Sender.Send(new GetCategoriesTreeQuery(), ct));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken ct)
    {
        return Respond(await Sender.Send(new GetCategoryByIdQuery(id), ct));
    }
}
