using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebApplication1
{
    /// <summary>
    /// Used to bind matrix parameter values from the URI.
    /// </summary>
    public class MatrixParameterAttribute : ModelBinderAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixParameterAttribute"/> class.
        /// </summary>
        /// <example>
        /// <c>[MatrixParameter] string[] color</c> will match all color values from the whole path.
        /// </example>
        public MatrixParameterAttribute(bool required = false) 
            : this(null, required)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixParameterAttribute"/> class.
        /// </summary>
        /// <param name="segment">
        /// Can be empty, a target prefix value, or a general route name embeded in "{" and "}".
        /// </param>
        /// <example>
        /// <c>[MatrixParam("")] string[] color</c> will match all color values from the whole path.
        /// <c>[MatrixParam("oranges")] string[] color</c> will match color only from the segment starting
        /// with "oranges" like .../oranges;color=red/...
        /// <c>[MatrixParam("{fruits}")] string[] color</c> will match color only from the route .../{fruits}/...
        /// </example>
        public MatrixParameterAttribute(string segment, bool required = false) 
            : base(typeof(MatrixParameterAttributeModelBinder)) => (Segment, Required) = (segment, required);

        /// <inheritdoc />
        public override BindingSource BindingSource => BindingSource.Path;

        /// <summary>
        /// Can be empty, a target prefix value, or a route parameter name embeded in "{" and "}".
        /// </summary>
        /// <example>
        /// <c>[MatrixParameter("")] string[] color</c> will match all color values from the whole path.
        /// <c>[MatrixParameter("oranges")] string[] color</c> will match color only from the segment starting
        /// with "oranges" like .../oranges;color=red/...
        /// <c>[MatrixParameter("{fruits}")] string[] color</c> will match color only from the route .../{fruits}/...
        /// </example>
        public string Segment { get; }

        /// <summary>
        /// Indicates that parameter is required for model binding.
        /// </summary>
        public bool Required { get; }
    }

    public class MatrixParameterAttributeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.ActionContext.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            {
                return Task.CompletedTask;
            }

            var matrixParameterDescriptor = actionDescriptor.Parameters
                .Cast<ControllerParameterDescriptor>()
                .FirstOrDefault(t => t.ParameterInfo.CustomAttributes.Any(c => c.AttributeType == typeof(MatrixParameterAttribute)));

            var matrixParameterAttribute = matrixParameterDescriptor.ParameterInfo
                .GetCustomAttributes(typeof(MatrixParameterAttribute), false)
                .OfType<MatrixParameterAttribute>()
                .FirstOrDefault();

            var segmentAttribute = matrixParameterAttribute.Segment;
            var modelName = context.OriginalModelName;

            // Match the route segment like [Route("{fruits}")] if possible.
            if (!string.IsNullOrEmpty(segmentAttribute)
                && segmentAttribute.StartsWith("{", StringComparison.Ordinal)
                && segmentAttribute.EndsWith("}", StringComparison.Ordinal))
            {
                var segmentName = segmentAttribute.Substring(1, segmentAttribute.Length - 2);
                var segmentResult = context.ValueProvider.GetValue(segmentName);
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
                    context.Model = attributeValues[0];
                    context.Result = ModelBindingResult.Success(context.Model);
                }

                return Task.CompletedTask;
            }

            ICollection<object> routeValues = context.ActionContext.RouteData.Values.Values;

            // Expand in case that a catch-all constraint will deliver a segment with "/" in it.
            var paramSegments = new List<string>();
            foreach (string segment in routeValues)
            {
                paramSegments.AddRange(segment.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
            }

            var collectedAttributeValues = new List<string>();
            foreach (string paramSegment in paramSegments)
            {
                // If no parameter is specified, as [MatrixParam], get values from all the segments.
                // If a segment prefix is specified like [MatrixParam("apples")], get values only it is matched.
                if (!string.IsNullOrEmpty(segmentAttribute) 
                    && !paramSegment.StartsWith($"{segmentAttribute};", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var attributeValues = GetAttributeValues(paramSegment, segmentAttribute);
                if (attributeValues != null)
                {
                    collectedAttributeValues.AddRange(attributeValues);
                }
            }

            context.Result = ModelBindingResult.Success(collectedAttributeValues.ToArray());
            return Task.CompletedTask;
        }

        private static IList<string> GetAttributeValues(string matrixParamSegment, string attributeName)
        {
            var valuesCollection = HttpUtility.ParseQueryString(matrixParamSegment.Replace(";", "&"));
            string attributeValueList = valuesCollection.Get(attributeName);
            if (attributeValueList is null)
            {
                return null;
            }

            return attributeValueList.Split(',');
        }
    }
}
