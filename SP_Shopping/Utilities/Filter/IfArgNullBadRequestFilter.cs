using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SP_Shopping.Utilities.Filter;

public class IfArgNullBadRequestFilter
(
    string argument,
    string? errorMessage = null
) : ActionFilterAttribute, IFilterFactory
{
    public bool IsReusable => false;

    public readonly string argument = argument;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IfArgNullBadRequestFilter>>();
        return new IfArgNullBadRequestFilterInner(logger, argument, errorMessage ?? $"{argument} is a required argument");
    }

    private class IfArgNullBadRequestFilterInner
    (
        ILogger<IfArgNullBadRequestFilter> logger,
        string argument,
        string errorMessage
    ) : ActionFilterAttribute
    {
        private readonly ILogger<IfArgNullBadRequestFilter> _logger = logger;
        private readonly string _argument = argument;
        private readonly string _errorMessage = errorMessage;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if
            (
                context.ActionArguments.TryGetValue(_argument, out var argumentValue) 
                && 
                argumentValue is not null
            )
            {
                base.OnActionExecuting(context);
            }
            else
            {
                _logger.LogWarning("{Argument} argument is null, returning {TypeName}", _argument, nameof(BadRequestObjectResult));
                context.Result = new BadRequestObjectResult(_errorMessage);
            }
        }
    }
}

// Copilot
