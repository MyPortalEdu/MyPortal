using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyPortal.Common.Enums;
using MyPortal.Common.Options;
using MyPortal.FileStorage.Helpers;
using MyPortal.FileStorage.Interfaces;
using MyPortal.FileStorage.Providers;

namespace MyPortal.FileStorage.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IStorageKeyGenerator, DefaultStorageKeyGenerator>();

            services.AddSingleton<IFileStorageProvider>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<FileStorageOptions>>().Value;

                return options.Provider switch
                {
                    FileStorageProvider.FileSystem =>
                        new FileSystemStorageProvider(sp.GetRequiredService<IOptions<FileStorageOptions>>()),

                    FileStorageProvider.AzureBlob =>
                        new AzureBlobStorageProvider(sp.GetRequiredService<IOptions<FileStorageOptions>>()),

                    _ => throw new InvalidOperationException($"Unknown FileStorage Provider: {options.Provider}")
                };
            });

            return services;
        }
    }
}
