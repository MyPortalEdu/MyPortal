using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QueryKit.Repositories.Filtering;

namespace MyPortal.WebApi.Models.Binders
{
    public class Base64JsonBinder<T>: IModelBinder where T : class
    {
        private readonly JsonSerializerOptions _json;

        public Base64JsonBinder(IOptions<JsonOptions> jsonOptions)
        {
            _json = jsonOptions.Value.JsonSerializerOptions;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var key = bindingContext.ModelName;

            var value = bindingContext.ValueProvider.GetValue(key);

            if (value == ValueProviderResult.None)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }

            var base64 = value.FirstValue;

            if (string.IsNullOrEmpty(base64))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            else
            {
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
                catch (Exception e)
                {
                    bindingContext.ModelState.TryAddModelError(key, $"Invalid {key} payload.");
                    bindingContext.Result = ModelBindingResult.Failed();
                }
            }
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

                // numbers: choose a sensible default; you can change to int/long/decimal policy
                JsonValueKind.Number => e.TryGetInt64(out var l) ? l
                    : e.TryGetDecimal(out var d) ? d
                    : e.GetDouble(),

                JsonValueKind.String => e.TryGetDateTime(out var dt) ? dt : e.GetString(),

                // for In/Between, you likely want arrays of primitive values
                JsonValueKind.Array => e.EnumerateArray().Select(ConvertJsonElement).ToArray(),

                // if you ever receive an object, decide: raw json string, or reject
                JsonValueKind.Object => e.GetRawText(),

                _ => e.GetRawText()
            };
        }
    }
}
