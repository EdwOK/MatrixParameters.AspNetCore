using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.MatrixParameter.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Microsoft.AspNetCore.MatrixParameter.ModelBinders
{
    public class SegmentPrefixAttributeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

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
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext is null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

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

            var value = segmentValue.Split(new[] { ";" }, 2, StringSplitOptions.None).First();
            bindingContext.Result = bindingContext.CreateResult(new []{ value });

            return Task.CompletedTask;
        }
    }
}