using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace WebApplication1
{
    public class MatrixParameterAttributeModelBinderProvider : IModelBinderProvider
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

            var attribute = metadata.Attributes.Attributes.OfType<MatrixParameterAttribute>().FirstOrDefault();
            if (attribute is null)
            {
                return null;
            }

            return new MatrixParameterAttributeModelBinder(attribute.Segment);
        }
    }
    
    public class MatrixParameterAttributeModelBinder : IModelBinder
    {
        private readonly string? _attributeSegment;

        public MatrixParameterAttributeModelBinder(string? segment) =>
            _attributeSegment = segment;

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext is null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            
            var modelName = bindingContext.OriginalModelName;

            // Match the route segment like [Route("{fruits}")] if possible.
            if (!string.IsNullOrEmpty(_attributeSegment)
                && _attributeSegment.StartsWith("{", StringComparison.Ordinal)
                && _attributeSegment.EndsWith("}", StringComparison.Ordinal))
            {
                var segmentName = _attributeSegment.Substring(1, _attributeSegment.Length - 2);

                var segmentResult = bindingContext.ValueProvider.GetValue(segmentName);
                if (segmentResult == ValueProviderResult.None)
                {
                    return Task.CompletedTask;
                }

                var matrixParamSegment = segmentResult.FirstValue;
                if (matrixParamSegment is null)
                {
                    return Task.CompletedTask;
                }

                var attributeValues = GetAttributeValues(matrixParamSegment, modelName);
                if (attributeValues is not null)
                {
                    bindingContext.Result = bindingContext.CreateResult(attributeValues);
                }

                return Task.CompletedTask;
            }

            var routeValues = bindingContext.ActionContext.RouteData.Values.Values.OfType<string>();

            // Expand in case that a catch-all constraint will deliver a segment with "/" in it.
            var paramSegments = new List<string>();
            foreach (var segment in routeValues)
            {
                paramSegments.AddRange(segment.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
            }

            var collectedAttributeValues = new List<string>();
            foreach (var paramSegment in paramSegments)
            {
                // If no parameter is specified, as [MatrixParameter], get values from all the segments.
                // If a segment prefix is specified like [MatrixParameter("apples")], get values only it is matched.
                if (!string.IsNullOrEmpty(_attributeSegment)
                    && !paramSegment.StartsWith($"{_attributeSegment};", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var attributeValues = GetAttributeValues(paramSegment, modelName);
                if (attributeValues is not null)
                {
                    collectedAttributeValues.AddRange(attributeValues);
                }
            }
            
            bindingContext.Result = bindingContext.CreateResult(collectedAttributeValues);
            return Task.CompletedTask;

            static IList<string>? GetAttributeValues(string matrixParamSegment, string attributeName)
            {
                var valuesCollection = HttpUtility.ParseQueryString(matrixParamSegment.Replace(";", "&").Replace("+", "%2B"));
                var attributeValueList = valuesCollection.Get(attributeName);
                return attributeValueList?.Split(',');
            }
        }
    }
}