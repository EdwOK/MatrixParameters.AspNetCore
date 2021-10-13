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
                var openApiParametersToChange =
                    new Dictionary<KeyValuePair<OperationType, OpenApiOperation>, IEnumerable<OpenApiParameter>>();

                foreach (var openApiOperation in path.Value.Operations)
                {
                    var matrixParameters = openApiOperation.Value.Parameters
                        .Where(p => p.Style == ParameterStyle.Matrix)
                        .ToArray();
                    if (matrixParameters.Length > 0)
                    {
                        openApiParametersToChange.Add(openApiOperation, matrixParameters);
                    }
                }

                if (openApiParametersToChange.Count == 0)
                {
                    newPaths.Add(path.Key, path.Value);
                    continue;
                }

                foreach (var (openApiOperation, parameters) in openApiParametersToChange)
                {
                    foreach (var parameter in parameters)
                    {
                        var newPathKey = $"{path.Key}{{{parameter.Name}}}";
                        if (newPaths.TryGetValue(newPathKey, out var newPath))
                        {
                            newPath.Operations.TryAdd(openApiOperation.Key, openApiOperation.Value);
                        }
                        else
                        {
                            newPaths.Add(newPathKey, new OpenApiPathItem
                            {
                                Extensions = path.Value.Extensions,
                                Parameters = path.Value.Parameters,
                                Servers = path.Value.Servers,
                                Summary = path.Value.Summary,
                                Operations = new Dictionary<OperationType, OpenApiOperation>
                                {
                                    { openApiOperation.Key, openApiOperation.Value }
                                },
                                Description = path.Value.Description,
                            });
                        }
                    }
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
