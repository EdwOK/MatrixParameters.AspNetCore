using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    /// <summary>
    /// Used to bind the segment prefix value from the route.
    /// </summary>
    /// <example>
    /// If [Route["{fruits}/{location}"] is specified and the incoming uri's relative path is
    /// "/apples:color=red,green/washington;rate=good", then in the action's argument list,
    /// <c>[SegmentPrefix] string fruits</c> will have fruits = apples
    /// but <c>string location</c> without this attribute will have location = washington;rate=good.
    /// </example>
    public class SegmentPrefixAttribute : ModelBinderAttribute
    {
        public SegmentPrefixAttribute() : base(typeof(SegmentPrefixAttributeModelBinder))
        {
        }

        /// <inheritdoc />
        public override BindingSource BindingSource => BindingSource.Path;
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

            bindingContext.ModelState.SetModelValue(segmentName, segmentResult);

            var segmentValue = segmentResult.FirstValue;
            if (segmentValue is null)
            {
                return Task.CompletedTask;
            }

            bindingContext.Model = segmentValue.Split(new[] { ";" }, 2, StringSplitOptions.None).First();
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);

            return Task.CompletedTask;
        }
    }
}
