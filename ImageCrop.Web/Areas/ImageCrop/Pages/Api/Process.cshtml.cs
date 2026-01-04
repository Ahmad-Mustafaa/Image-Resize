using ImageCrop.Web.Models;
using ImageCrop.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ImageCrop.Web.Areas.ImageCrop.Pages.Api;

public class ProcessModel : PageModel
{
    private readonly IImageProcessingService _imageService;
    private readonly ILogger<ProcessModel> _logger;

    public ProcessModel(IImageProcessingService imageService, ILogger<ProcessModel> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<IActionResult> OnPostAsync([FromBody] ProcessImageRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ImageId))
            {
                return BadRequest(new { success = false, message = "Image ID is required" });
            }

            // Find the image file
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var imageFiles = Directory.GetFiles(uploadsPath, $"{request.ImageId}.*");

            if (imageFiles.Length == 0)
            {
                return NotFound(new { success = false, message = "Image not found" });
            }

            var imagePath = imageFiles[0];

            // Process the image
            var result = await _imageService.ProcessImageAsync(imagePath, request);

            return new JsonResult(new
            {
                success = true,
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image");
            return new JsonResult(new
            {
                success = false,
                message = $"Error processing image: {ex.Message}"
            })
            {
                StatusCode = 500
            };
        }
    }
}
