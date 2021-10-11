using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebApplication1
{
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
                if (!segmentResult.Any())
                {
                    return Task.CompletedTask;
                }

                var segmentValue = segmentResult.FirstValue;
                if (segmentValue != null)
                {
                    bindingContext.Model = segmentValue.Split(new[] { ";" }, 2, StringSplitOptions.None).First();
                }
                else
                {
                    bindingContext.Model = segmentValue;
                }

                bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);

                return Task.CompletedTask;
            }
        }
    }

    public class MatrixParameterAttribute : ModelBinderAttribute
    {
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
                var modelName = bindingContext.ModelName;

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
        }
    }
}
