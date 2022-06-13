using MatrixParameters.AspNetCore.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace MatrixParameters.AspNetCore.ModelBinders;

public class SegmentPrefixAttributeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata is not DefaultModelMetadata metadata)
        {
            return null;
        }

        if (metadata.Attributes.Attributes.All(a => a.GetType() != typeof(SegmentPrefixAttribute)))
        {
            return null;
        }

        return new SegmentPrefixAttributeModelBinder();
    }
}
    
public class SegmentPrefixAttributeModelBinder : IModelBinder
{
    private const string KeyDelimiter = ";";

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var segmentName = bindingContext.ModelName;

        var segmentResult = bindingContext.ValueProvider.GetValue(segmentName);
        if (segmentResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var segmentValue = segmentResult.FirstValue;
        if (segmentValue is null)
        {
            return Task.CompletedTask;
        }

        var value = segmentValue.Split(new[] { KeyDelimiter }, 2, StringSplitOptions.None).First();
        bindingContext.Result = bindingContext.CreateResult(new[] { value });

        return Task.CompletedTask;
    }
}