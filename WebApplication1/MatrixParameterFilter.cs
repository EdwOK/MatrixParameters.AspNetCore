using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

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

            var isMatrixParameter = context.ParameterInfo?.CustomAttributes
                ?.Any(a => a.AttributeType == typeof(MatrixParameterAttribute));
            if (isMatrixParameter.GetValueOrDefault(false))
            {
                parameter.Style = ParameterStyle.Matrix;
            }
        }
    }
}
