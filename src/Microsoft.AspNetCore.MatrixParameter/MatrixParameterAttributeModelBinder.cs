using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.MatrixParameter
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
        private readonly string? _matrixParameterSegment;

        public MatrixParameterAttributeModelBinder(string? matrixParameterSegment) =>
            _matrixParameterSegment = matrixParameterSegment;

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext is null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.OriginalModelName;

            // Match the route segment like [Route("{fruits}")] if possible.
            if (!string.IsNullOrEmpty(_matrixParameterSegment)
                && _matrixParameterSegment.StartsWith("{", StringComparison.Ordinal)
                && _matrixParameterSegment.EndsWith("}", StringComparison.Ordinal))
            {
                var segmentName = _matrixParameterSegment.Substring(1, _matrixParameterSegment.Length - 2);

                var segmentResult = bindingContext.ValueProvider.GetValue(segmentName);
                if (segmentResult == ValueProviderResult.None)
                {
                    return Task.CompletedTask;
                }

                var matrixParameterSegment = segmentResult.FirstValue;
                if (matrixParameterSegment is null)
                {
                    return Task.CompletedTask;
                }

                var modelValues = GetModelValues(matrixParameterSegment, modelName);
                if (modelValues is not null)
                {
                    bindingContext.Result = bindingContext.CreateResult(modelValues);
                }

                return Task.CompletedTask;
            }

            var routeValues = bindingContext.ActionContext.RouteData.Values.Values.OfType<string>();

            // Expand in case that a catch-all constraint will deliver a segment with "/" in it.
            var parameterSegments = new List<string>();
            foreach (var segment in routeValues)
            {
                parameterSegments.AddRange(segment.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
            }

            var collectedAttributeValues = new List<string>();
            foreach (string parameterSegment in parameterSegments)
            {
                // If no parameter is specified, as [MatrixParameter], get values from all the segments.
                // If a segment prefix is specified like [MatrixParameter("apples")], get values only it is matched.
                if (!string.IsNullOrEmpty(_matrixParameterSegment)
                    && !parameterSegment.StartsWith($"{_matrixParameterSegment};", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var modelValues = GetModelValues(parameterSegment, modelName);
                if (modelValues is not null)
                {
                    collectedAttributeValues.AddRange(modelValues);
                }
            }

            bindingContext.Result = bindingContext.CreateResult(collectedAttributeValues);
            return Task.CompletedTask;

            static IEnumerable<string>? GetModelValues(string matrixParameterSegment, string matrixParameterName)
            {
                var queryString = $"?{matrixParameterSegment.Replace(";", "&").Replace("+", "%2B")}";

                var valuesCollection = QueryHelpers.ParseQuery(queryString);
                if (!valuesCollection.TryGetValue(matrixParameterName, out var attributeValueList))
                {
                    return null;
                }

                if (attributeValueList.Count == 0)
                {
                    return null;
                }
                
                return attributeValueList;
            }
        }
    }
}