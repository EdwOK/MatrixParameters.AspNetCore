using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebApplication1
{
    public class MatrixDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var newPaths = new OpenApiPaths();
            foreach (var path in swaggerDoc.Paths)
            {
                var paramsToChange = new List<string>();

                foreach (var openApiOperation in path.Value.Operations.Values)
                {
                    paramsToChange.AddRange(openApiOperation.Parameters.Where(x => x.Style == ParameterStyle.Matrix).Select(x => x.Name));
                }

                if (paramsToChange.Any())
                {
                    newPaths.Add($"{path.Key}{string.Join("", paramsToChange.Select(x => $"{{{x}}}"))}", path.Value);
                }
                else
                {
                    newPaths.Add(path.Key, path.Value);
                }
            }

            swaggerDoc.Paths = newPaths;
        }
    }

    public class MatrixParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (!parameter.In.HasValue || parameter.In.Value != ParameterLocation.Path)
            {
                return;
            }

            var isMatrixStyle = context.ParameterInfo?.CustomAttributes
                .Any(a => a.AttributeType == typeof(MatrixParameterAttribute));

            if (!isMatrixStyle.GetValueOrDefault())
            {
                return;
            }

            parameter.Style = ParameterStyle.Matrix;
            parameter.Explode = true;
            parameter.Required = false;
        }
    }

    public class SegmentPrefixAttribute : ModelBinderAttribute
    {
        public override BindingSource BindingSource => BindingSource.Path;

        public SegmentPrefixAttribute() : base(typeof(SegmentPrefixAttributeModelBinder))
        {
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
                if (segmentValue != null)
                {
                    bindingContext.Model = segmentValue.Split(new[] { ";" }, 2, StringSplitOptions.None).First();
                }
                else
                {
                    return Task.CompletedTask;
                }

                bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);

                return Task.CompletedTask;
            }
        }
    }

    public class MatrixParameterAttribute : ModelBinderAttribute
    {
        public MatrixParameterAttribute() : this(null)
        {
        }

        public MatrixParameterAttribute(string segment) : base(typeof(MatrixParameterAttributeModelBinder))
        {
            Segment = segment;
        }

        public string Segment { get; }

        public override BindingSource BindingSource => BindingSource.Path;

        public class MatrixParameterAttributeModelBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext is null)
                {
                    throw new ArgumentNullException(nameof(bindingContext));
                }

                var actionDescriptor = bindingContext.ActionContext.ActionDescriptor as ControllerActionDescriptor;
                var parameterDescriptor = actionDescriptor.Parameters
                    .Cast<ControllerParameterDescriptor>()
                    .FirstOrDefault(t => t.ParameterInfo.CustomAttributes.Any(c => c.AttributeType == typeof(MatrixParameterAttribute)));

                var parameterAttribute = parameterDescriptor.ParameterInfo
                    .GetCustomAttributes(typeof(MatrixParameterAttribute), false)
                    .OfType<MatrixParameterAttribute>()
                    .FirstOrDefault();

                var _segment = parameterAttribute.Segment;
                var modelName = bindingContext.OriginalModelName;

                // Match the route segment like [Route("{fruits}")] if possible.
                if (!String.IsNullOrEmpty(_segment)
                    && _segment.StartsWith("{", StringComparison.Ordinal)
                    && _segment.EndsWith("}", StringComparison.Ordinal))
                {
                    string segmentName = _segment.Substring(1, _segment.Length - 2);
                    ValueProviderResult segmentResult = bindingContext.ValueProvider.GetValue(segmentName);
                    if (segmentResult == ValueProviderResult.None)
                    {
                        return Task.CompletedTask;
                    }

                    string matrixParamSegment = segmentResult.FirstValue;
                    if (matrixParamSegment == null)
                    {
                        return Task.CompletedTask;
                    }

                    IList<string> attributeValues = GetAttributeValues(matrixParamSegment, modelName);
                    if (attributeValues != null)
                    {
                        bindingContext.Model = attributeValues[0];
                        bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
                    }

                    return Task.CompletedTask;
                }

                ICollection<object> routeValues = bindingContext.ActionContext.RouteData.Values.Values;

                // Expand in case that a catch-all constraint will deliver a segment with "/" in it.
                var paramSegments = new List<string>();
                foreach (string segment in routeValues)
                {
                    paramSegments.AddRange(segment.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
                }

                List<string> collectedAttributeValues = new List<string>();
                foreach (string paramSegment in paramSegments)
                {
                    // If no parameter is specified, as [MatrixParam], get values from all the segments.
                    // If a segment prefix is specified like [MatrixParam("apples")], get values only it is matched.
                    if (!String.IsNullOrEmpty(_segment)
                        && !paramSegment.StartsWith(_segment + ";", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var valuesCollection = HttpUtility.ParseQueryString(paramSegment.Replace(";", "&"));
                    var attributeValue = valuesCollection.Get(modelName);
                    if (attributeValue is not null)
                    {
                        collectedAttributeValues.Add(attributeValue);
                    }
                }

                bindingContext.Result = ModelBindingResult.Success(collectedAttributeValues.ToArray());
                return Task.CompletedTask;
            }

            private IList<string> GetAttributeValues(string matrixParamSegment, string attributeName)
            {
                NameValueCollection valuesCollection =
                    HttpUtility.ParseQueryString(matrixParamSegment.Replace(";", "&"));
                string attributeValueList = valuesCollection.Get(attributeName);
                if (attributeValueList == null)
                {
                    return null;
                }

                return attributeValueList.Split(',');
            }
        }
    }
}
