using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EM2Devs.Todo.Api.ModelBinding;

public sealed class DateOnlyModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        string? value = valueProviderResult.FirstValue;

        if (string.IsNullOrWhiteSpace(value))
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                $"Invalid {bindingContext.ModelName} format. Expected: yyyy-MM-dd");
            return Task.CompletedTask;
        }

        if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
        {
            bindingContext.Result = ModelBindingResult.Success(date);
        }
        else
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                $"Invalid {bindingContext.ModelName} format. Expected: yyyy-MM-dd");
        }

        return Task.CompletedTask;
    }
}

public sealed class DateOnlyModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(DateOnly) || context.Metadata.ModelType == typeof(DateOnly?))
        {
            return new DateOnlyModelBinder();
        }

        return null;
    }
}
