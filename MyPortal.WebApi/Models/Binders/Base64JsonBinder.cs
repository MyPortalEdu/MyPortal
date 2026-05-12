using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QueryKit.Repositories.Filtering;

namespace MyPortal.WebApi.Models.Binders
{
    public class Base64JsonBinder<T> : IModelBinder where T : class
    {
        private const int MaxBase64Length = 8 * 1024;
        private const int MaxJsonDepth = 16;

        private readonly JsonSerializerOptions _json;

        public Base64JsonBinder(IOptions<JsonOptions> jsonOptions)
        {
            _json = new JsonSerializerOptions(jsonOptions.Value.JsonSerializerOptions)
            {
                MaxDepth = MaxJsonDepth
            };
            // QueryKit's FilterOperator / SortDirection / BoolJoin are plain
            // enums and System.Text.Json otherwise insists on numeric input,
            // which would force every JS caller to hard-code the integer order
            // of QueryKit's enum members (and silently break if they ever
            // reorder). Accepting string names here is local to filter/sort
            // binding — response serialization elsewhere keeps numeric enums.
            _json.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: true));
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var key = bindingContext.ModelName;
            var value = bindingContext.ValueProvider.GetValue(key);

            if (value == ValueProviderResult.None)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var base64 = value.FirstValue;

            if (string.IsNullOrEmpty(base64))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            if (base64.Length > MaxBase64Length)
            {
                bindingContext.ModelState.TryAddModelError(key, $"{key} payload exceeds maximum size.");
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            try
            {
                var json = Base64UrlEncoder.Decode(base64);
                var model = JsonSerializer.Deserialize<T>(json, _json);

                if (model is FilterOptions fo)
                {
                    NormalizeFilterValues(fo);
                }

                bindingContext.Result = ModelBindingResult.Success(model);
            }
            catch (Exception)
            {
                bindingContext.ModelState.TryAddModelError(key, $"Invalid {key} payload.");
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }

        private static void NormalizeFilterValues(FilterOptions filter)
        {
            if (filter.Groups is null || filter.Groups.Length == 0) return;

            foreach (var g in filter.Groups)
            {
                if (g?.Criteria is null || g.Criteria.Length == 0) continue;

                foreach (var c in g.Criteria)
                {
                    if (c is null) continue;

                    if (c.Value is JsonElement je)
                    {
                        c.Value = ConvertJsonElement(je);
                    }
                }
            }
        }

        private static object? ConvertJsonElement(JsonElement e)
        {
            return e.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => e.TryGetInt64(out var l) ? l
                    : e.TryGetDecimal(out var d) ? d
                    : e.GetDouble(),
                JsonValueKind.String => e.GetString(),
                JsonValueKind.Array => e.EnumerateArray().Select(ConvertJsonElement).ToArray(),
                JsonValueKind.Object => throw new FormatException("Object values are not allowed in filter criteria."),
                _ => throw new FormatException($"Unsupported filter value kind '{e.ValueKind}'.")
            };
        }
    }
}
