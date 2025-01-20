using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Teniry.Cqrs.Extended.Queryables.Sort.Swagger;

public class SwaggerShowAvailableValuesForSort : IParameterFilter {
    /// <inheritdoc />
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context) {
        if (context.ParameterInfo == null
            || !parameter.Name.Equals("sort", StringComparison.CurrentCultureIgnoreCase)
            || context.ParameterInfo.Member.ReflectedType?.GetInterface(nameof(IDefineSortable)) == null) {
            return;
        }

        if (Activator.CreateInstance(context.ParameterInfo.Member.ReflectedType) is IDefineSortable instance) {
            parameter.Schema.Items = new() {
                Type = "string",
                Enum = instance.GetSortKeysWithDirection().Select(x => new OpenApiString(x)).ToList<IOpenApiAny>()
            };
        }
    }
}