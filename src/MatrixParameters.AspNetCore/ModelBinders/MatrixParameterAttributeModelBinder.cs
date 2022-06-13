using System.Web;
using MatrixParameters.AspNetCore.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace MatrixParameters.AspNetCore.ModelBinders;

public class MatrixParameterAttributeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

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

    private const string SeparatorString = "/";
    private const string KeyDelimiter = ";";
    private const string OpenBrace = "{";
    private const string CloseBrace = "}";
    private const string Comma = ",";
    private const string Ampersand = "&";
    private const string PlusSign = "+";
    private const string DecodedPlusSign = "%2B";

    public MatrixParameterAttributeModelBinder(string? matrixParameterSegment) =>
        _matrixParameterSegment = matrixParameterSegment;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.OriginalModelName;

        // Match the route segment like [Route("{fruits}")] if possible.
        if (!string.IsNullOrEmpty(_matrixParameterSegment)
            && _matrixParameterSegment.StartsWith(OpenBrace, StringComparison.Ordinal)
            && _matrixParameterSegment.EndsWith(CloseBrace, StringComparison.Ordinal))
        {
            var segmentName = _matrixParameterSegment[1..^1];

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

            var attributeValues = GetAttributeValues(matrixParameterSegment, modelName);
            if (attributeValues is not null)
            {
                bindingContext.Result = bindingContext.CreateResult(attributeValues);
            }

            return Task.CompletedTask;
        }

        // Expand in case that a catch-all constraint will deliver a segment with "/" in it.
        var routeValues = bindingContext.ActionContext.RouteData.Values.Values.OfType<string>();
        var parameterSegments = new List<string>();
        foreach (var segment in routeValues)
        {
            parameterSegments.AddRange(segment.Split(new[] { SeparatorString }, StringSplitOptions.RemoveEmptyEntries));
        }

        var attributeValuesCollection = new List<string>();
        foreach (var parameterSegment in parameterSegments)
        {
            // If no parameter is specified, as [MatrixParameter], get values from all the segments.
            // If a segment prefix is specified like [MatrixParameter("apples")], get values only it is matched.
            if (!string.IsNullOrEmpty(_matrixParameterSegment)
                && !parameterSegment.StartsWith($"{_matrixParameterSegment}{KeyDelimiter}", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var attributeValues = GetAttributeValues(parameterSegment, modelName);
            if (attributeValues is not null)
            {
                attributeValuesCollection.AddRange(attributeValues);
            }
        }

        bindingContext.Result = bindingContext.CreateResult(attributeValuesCollection);
        return Task.CompletedTask;

        static IList<string>? GetAttributeValues(string matrixParamSegment, string attributeName)
        {
            var valuesCollection =
                HttpUtility.ParseQueryString(matrixParamSegment
                    .Replace(KeyDelimiter, Ampersand)
                    .Replace(PlusSign, DecodedPlusSign));
            
            var attributeValues = valuesCollection.Get(attributeName)?.Split(Comma)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
            
            return attributeValues?.Length == 0 ? null : attributeValues;
        }
    }
}