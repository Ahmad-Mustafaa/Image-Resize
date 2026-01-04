using ImageCrop.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ImageCrop.Web;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImageCrop(this IServiceCollection services)
    {
        services.AddScoped<IImageProcessingService, ImageProcessingService>();
        return services;
    }
}
