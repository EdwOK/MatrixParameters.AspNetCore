using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MatrixParameters.AspNetCore.Swagger;

public class MatrixDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var newPaths = new OpenApiPaths();

        foreach (var path in swaggerDoc.Paths)
        {
            var openApiParametersToChange =
                new Dictionary<KeyValuePair<OperationType, OpenApiOperation>, IEnumerable<OpenApiParameter>>();
            var openApiParametersWithoutChange =
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
                else
                {
                    openApiParametersWithoutChange.Add(openApiOperation, openApiOperation.Value.Parameters);
                }
            }

            foreach (var (openApiOperation, parameters) in openApiParametersWithoutChange)
            {
                if (newPaths.TryGetValue(path.Key, out var newPath))
                {
                    newPath.Operations.TryAdd(openApiOperation.Key, openApiOperation.Value);
                }
                else
                {
                    newPaths.Add(path.Key, new OpenApiPathItem
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

            foreach (var (openApiOperation, parameters) in openApiParametersToChange)
            {
                var parameterPaths = parameters.Aggregate(path.Key, (current, parameter) => $"{current}{{{parameter.Name}}}");

                if (newPaths.TryGetValue(parameterPaths, out var newPath))
                {
                    newPath.Operations.TryAdd(openApiOperation.Key, openApiOperation.Value);
                }
                else
                {
                    newPaths.Add(parameterPaths, new OpenApiPathItem
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

        swaggerDoc.Paths.Clear();
        foreach (var path in newPaths)
        {
            swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }
}