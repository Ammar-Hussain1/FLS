using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FLS_API.Utilities
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameters = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) || 
                           p.ParameterType == typeof(IFormFileCollection) ||
                           (p.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromFormAttribute), false).Any() &&
                            HasFormFileProperty(p.ParameterType)))
                .ToList();

            var formParameters = context.MethodInfo.GetParameters()
                .Where(p => p.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromFormAttribute), false).Any())
                .ToList();

            if (fileParameters.Any() || formParameters.Any())
            {
                var parametersToRemove = operation.Parameters
                    .Where(p => formParameters.Any(fp => fp.Name == p.Name))
                    .ToList();

                foreach (var param in parametersToRemove)
                {
                    operation.Parameters.Remove(param);
                }

                if (operation.RequestBody == null)
                {
                    operation.RequestBody = new OpenApiRequestBody();
                }

                operation.RequestBody.Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>()
                        }
                    }
                };

                foreach (var param in formParameters)
                {
                    if (HasFormFileProperty(param.ParameterType))
                    {
                        var properties = param.ParameterType.GetProperties();
                        foreach (var prop in properties)
                        {
                            OpenApiSchema schema;
                            if (prop.PropertyType == typeof(IFormFile) || prop.PropertyType == typeof(IFormFileCollection))
                            {
                                schema = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                };
                            }
                            else
                            {
                                schema = new OpenApiSchema
                                {
                                    Type = GetOpenApiType(prop.PropertyType)
                                };

                                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    schema.Nullable = true;
                                }
                            }

                            operation.RequestBody.Content["multipart/form-data"].Schema.Properties[prop.Name] = schema;
                        }
                    }
                    else if (param.ParameterType == typeof(IFormFile) || param.ParameterType == typeof(IFormFileCollection))
                    {
                        // Direct IFormFile parameter
                        var schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        };
                        operation.RequestBody.Content["multipart/form-data"].Schema.Properties[param.Name] = schema;
                    }
                    else
                    {
                        // Other form parameter
                        var schema = new OpenApiSchema
                        {
                            Type = GetOpenApiType(param.ParameterType)
                        };

                        if (param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            schema.Nullable = true;
                        }

                        operation.RequestBody.Content["multipart/form-data"].Schema.Properties[param.Name] = schema;
                    }
                }
            }
        }

        private bool HasFormFileProperty(Type type)
        {
            if (type == null) return false;
            return type.GetProperties()
                .Any(p => p.PropertyType == typeof(IFormFile) || p.PropertyType == typeof(IFormFileCollection));
        }

        private string GetOpenApiType(Type type)
        {
            if (type == typeof(string))
                return "string";
            if (type == typeof(int) || type == typeof(int?))
                return "integer";
            if (type == typeof(bool) || type == typeof(bool?))
                return "boolean";
            if (type == typeof(double) || type == typeof(float) || type == typeof(decimal))
                return "number";
            return "string";
        }
    }
}