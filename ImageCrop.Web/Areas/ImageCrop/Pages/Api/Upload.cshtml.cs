using ImageCrop.Web.Models;
using ImageCrop.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace ImageCrop.Web.Areas.ImageCrop.Pages.Api;

public class UploadModel : PageModel
{
    private readonly IImageProcessingService _imageService;
    private readonly ILogger<UploadModel> _logger;

    public UploadModel(IImageProcessingService imageService, ILogger<UploadModel> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<IActionResult> OnPostAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new JsonResult(new UploadResponse
                {
                    Success = false,
                    Message = "No file uploaded"
                });
            }

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return new JsonResult(new UploadResponse
                {
                    Success = false,
                    Message = "File size exceeds 10MB limit"
                });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return new JsonResult(new UploadResponse
                {
                    Success = false,
                    Message = "Invalid file type. Allowed: JPG, PNG, WebP, BMP, GIF"
                });
            }

            // Save the uploaded image
            var imageId = await _imageService.SaveUploadedImageAsync(file.OpenReadStream(), file.FileName);
            var imagePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads"), $"{imageId}{extension}");

            // Get image info
            var imageInfo = await _imageService.GetImageInfoAsync(imagePath, imageId);

            return new JsonResult(new UploadResponse
            {
                Success = true,
                Message = "Image uploaded successfully",
                ImageInfo = imageInfo
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return new JsonResult(new UploadResponse
            {
                Success = false,
                Message = $"Error uploading image: {ex.Message}"
            });
        }
    }
}
