using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

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
                    var matrixParameters = openApiOperation.Parameters
                        .Where(p => p.Style == ParameterStyle.Matrix)
                        .Select(p => p.Name);
                    paramsToChange.AddRange(matrixParameters);
                }

                if (paramsToChange.Any())
                {
                    var parametersPathKey = string.Join("", paramsToChange.Select(p => $"{{{p}}}"));
                    newPaths.Add($"{path.Key}{parametersPathKey}", path.Value);
                }
                else
                {
                    newPaths.Add(path.Key, path.Value);
                }
            }

            swaggerDoc.Paths.Clear();

            foreach (var path in newPaths)
            {
                swaggerDoc.Paths.Add(path.Key, path.Value);
            }
        }
    }
}
