using System.Linq;
using Microsoft.AspNetCore.MatrixParameter.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.AspNetCore.MatrixParameter.Swagger;

public class MatrixParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (parameter.In is not ParameterLocation.Path)
        {
            return;
        }

        var matrixParameterAttribute =
            context.ParameterInfo?.GetCustomAttributes(true)
                .OfType<MatrixParameterAttribute>()
                .FirstOrDefault();

        if (matrixParameterAttribute is not null)
        {
            parameter.Style = ParameterStyle.Matrix;
            parameter.AllowEmptyValue = !matrixParameterAttribute.Required;
            parameter.Required = matrixParameterAttribute.Required;
        }
    }
}