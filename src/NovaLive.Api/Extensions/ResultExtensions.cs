using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Contracts.Common;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Extensions;

public static class ResultExtensions
{
    public static ApiResponse<T> ToApiResponse<T>(this Result<T> result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return ApiResponse<T>.Ok(result.Value, successMessage);
        }

        var apiError = new ApiError
        {
            Code = result.Error.Code,
            Message = result.Error.Message ?? "Đã có lỗi xảy ra.",
            Type = result.Error.Type
        };

        if (result is IValidationResult validationResult && validationResult.Errors.Length > 0)
        {
            apiError.ValidationErrors = validationResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Message ?? string.Empty).ToArray());
        }

        return ApiResponse<T>.Fail(apiError);
    }

    public static ApiResponse<object> ToApiResponse(this Result result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return ApiResponse<object>.Ok(new object(), successMessage);
        }

        var apiError = new ApiError
        {
            Code = result.Error.Code,
            Message = result.Error.Message ?? "Đã có lỗi xảy ra.",
            Type = result.Error.Type
        };

        if (result is IValidationResult validationResult && validationResult.Errors.Length > 0)
        {
            apiError.ValidationErrors = validationResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Message ?? string.Empty).ToArray());
        }

        return ApiResponse<object>.Fail(apiError);
    }

    public static IResult ToIResult<T>(this Result<T> result, string? successMessage = null)
    {
        var response = result.ToApiResponse(successMessage);
        return result.IsSuccess
            ? Results.Ok(response)
            : Results.Json(response, statusCode: GetStatusCode(result.Error.Type));
    }

    public static IResult ToIResult(this Result result, string? successMessage = null)
    {
        var response = result.ToApiResponse(successMessage);
        return result.IsSuccess
            ? Results.Ok(response)
            : Results.Json(response, statusCode: GetStatusCode(result.Error.Type));
    }

    public static ActionResult ToActionResult<T>(this Result<T> result, string? successMessage = null)
    {
        var response = result.ToApiResponse(successMessage);
        return result.IsSuccess
            ? new OkObjectResult(response)
            : new ObjectResult(response) { StatusCode = GetStatusCode(result.Error.Type) };
    }

    public static ActionResult ToActionResult(this Result result, string? successMessage = null)
    {
        var response = result.ToApiResponse(successMessage);
        return result.IsSuccess
            ? new OkObjectResult(response)
            : new ObjectResult(response) { StatusCode = GetStatusCode(result.Error.Type) };
    }

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.AlreadyExists => StatusCodes.Status409Conflict,
        ErrorType.Invalid => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };
}
