using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Auth;

// A cross-origin browser must pass a CORS preflight before it can send this non-simple header.
[AttributeUsage(AttributeTargets.Method)]
public sealed class RequireCookieCsrfAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest") return;
        var problem = AuthProblem.Create(context.HttpContext,
            Error.Forbidden("Auth.CsrfRejected", "X-Requested-With: XMLHttpRequest is required."));
        context.Result = new ObjectResult(problem)
        {
            StatusCode = problem.Status, ContentTypes = { "application/problem+json" }
        };
    }
}
