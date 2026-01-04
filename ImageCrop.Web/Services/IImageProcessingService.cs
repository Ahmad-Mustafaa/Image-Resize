using ImageCrop.Web.Models;

namespace ImageCrop.Web.Services;

public interface IImageProcessingService
{
    Task<ProcessedImageResponse> ProcessImageAsync(string imagePath, ProcessImageRequest request);
    Task<ImageInfoResponse> GetImageInfoAsync(string imagePath, string imageId);
    Task<string> SaveUploadedImageAsync(Stream imageStream, string fileName);
}
