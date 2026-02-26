using FluentValidation;

namespace ThrPresetsApi.Api.Common.Filters;

public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null) return await next(context);
        
        var dto = context.Arguments.OfType<T>().FirstOrDefault();
        if (dto is null) return await next(context);
        
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        return await next(context);
    }
}