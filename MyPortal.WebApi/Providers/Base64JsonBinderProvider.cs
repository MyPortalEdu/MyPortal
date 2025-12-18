using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MyPortal.WebApi.Models.Binders;

namespace MyPortal.WebApi.Providers
{
    public sealed class Base64JsonBinderProvider<T> : IModelBinderProvider where T : class
    {
        private readonly JsonSerializerOptions _json;

        public Base64JsonBinderProvider(JsonSerializerOptions? json = null)
        {
            _json = json ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.BindingInfo.BindingSource is not null &&
                context.BindingInfo.BindingSource != BindingSource.Query)
            {
                return null;
            }

            if (context.Metadata.ModelType != typeof(T))
            {
                return null;
            }

            return new BinderTypeModelBinder(typeof(Base64JsonBinder<T>));
        }
    }
}
