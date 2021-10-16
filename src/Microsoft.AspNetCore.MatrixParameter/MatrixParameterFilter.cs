using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.AspNetCore.MatrixParameter
{
    public class MatrixParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (parameter.In is not ParameterLocation.Path)
            {
                return;
            }

            var isAnyMatrixParams =
                context.ParameterInfo?.CustomAttributes.Any(a => a.AttributeType == typeof(MatrixParameterAttribute));
            
            if (isAnyMatrixParams.GetValueOrDefault())
            {
                parameter.Style = ParameterStyle.Matrix;
            }
        }
    }
}
