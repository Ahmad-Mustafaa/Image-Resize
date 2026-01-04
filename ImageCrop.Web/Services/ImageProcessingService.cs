using ImageCrop.Web.Models;
using SkiaSharp;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace ImageCrop.Web.Services;

public class ImageProcessingService : IImageProcessingService
{
    private readonly string _uploadPath;
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(IWebHostEnvironment environment, ILogger<ImageProcessingService> logger)
    {
        _uploadPath = Path.Combine(environment.WebRootPath, "uploads");
        _logger = logger;
        
        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<string> SaveUploadedImageAsync(Stream imageStream, string fileName)
    {
        var imageId = Guid.NewGuid().ToString();
        var extension = Path.GetExtension(fileName);
        var filePath = Path.Combine(_uploadPath, $"{imageId}{extension}");

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await imageStream.CopyToAsync(fileStream);
        }

        return imageId;
    }

    public async Task<ImageInfoResponse> GetImageInfoAsync(string imagePath, string imageId)
    {
        return await Task.Run(() =>
        {
            using var bitmap = SKBitmap.Decode(imagePath);
            if (bitmap == null)
            {
                throw new InvalidOperationException("Failed to decode image");
            }

            var fileInfo = new FileInfo(imagePath);
            var extension = Path.GetExtension(imagePath).TrimStart('.').ToLower();

            return new ImageInfoResponse
            {
                ImageId = imageId,
                Width = bitmap.Width,
                Height = bitmap.Height,
                Format = extension,
                FileSizeBytes = fileInfo.Length,
                PreviewUrl = $"/uploads/{Path.GetFileName(imagePath)}"
            };
        });
    }

    public async Task<ProcessedImageResponse> ProcessImageAsync(string imagePath, ProcessImageRequest request)
    {
        return await Task.Run(() =>
        {
            using var originalBitmap = SKBitmap.Decode(imagePath);
            if (originalBitmap == null)
            {
                throw new InvalidOperationException("Failed to decode image");
            }

            SKBitmap processedBitmap = originalBitmap;
            bool needsDisposal = false;

            try
            {
                // Step 1: Crop if requested
                if (request.Crop != null)
                {
                    var cropRect = new SKRectI(
                        request.Crop.X,
                        request.Crop.Y,
                        request.Crop.X + request.Crop.Width,
                        request.Crop.Y + request.Crop.Height
                    );

                    var croppedBitmap = new SKBitmap(request.Crop.Width, request.Crop.Height);
                    using (var canvas = new SKCanvas(croppedBitmap))
                    {
                        canvas.Clear(SKColors.Transparent);
                        var sourceRect = new SKRect(cropRect.Left, cropRect.Top, cropRect.Right, cropRect.Bottom);
                        var destRect = new SKRect(0, 0, request.Crop.Width, request.Crop.Height);
                        var paint = new SKPaint
                        {
                            IsAntialias = true
                        };
                        canvas.DrawBitmap(originalBitmap, sourceRect, destRect, paint);
                    }

                    if (needsDisposal) processedBitmap.Dispose();
                    processedBitmap = croppedBitmap;
                    needsDisposal = true;
                }

                // Step 2: Resize if requested
                if (request.Resize != null && (request.Resize.Width.HasValue || request.Resize.Height.HasValue))
                {
                    int targetWidth, targetHeight;

                    if (request.Resize.MaintainAspectRatio)
                    {
                        var aspectRatio = (double)processedBitmap.Width / processedBitmap.Height;

                        if (request.Resize.Width.HasValue && !request.Resize.Height.HasValue)
                        {
                            targetWidth = request.Resize.Width.Value;
                            targetHeight = (int)(targetWidth / aspectRatio);
                        }
                        else if (request.Resize.Height.HasValue && !request.Resize.Width.HasValue)
                        {
                            targetHeight = request.Resize.Height.Value;
                            targetWidth = (int)(targetHeight * aspectRatio);
                        }
                        else
                        {
                            targetWidth = request.Resize.Width!.Value;
                            targetHeight = request.Resize.Height!.Value;
                        }
                    }
                    else
                    {
                        targetWidth = request.Resize.Width ?? processedBitmap.Width;
                        targetHeight = request.Resize.Height ?? processedBitmap.Height;
                    }

                    var resizedBitmap = processedBitmap.Resize(new SKImageInfo(targetWidth, targetHeight), SKSamplingOptions.Default);
                    if (resizedBitmap == null)
                    {
                        throw new InvalidOperationException("Failed to resize image");
                    }

                    if (needsDisposal) processedBitmap.Dispose();
                    processedBitmap = resizedBitmap;
                    needsDisposal = true;
                }

                // Step 3: Convert format and compress
                var format = GetImageFormat(request.Convert?.TargetFormat ?? "png");
                var quality = request.Compress?.Quality ?? request.Convert?.Quality ?? 90;

                using var image = SKImage.FromBitmap(processedBitmap);
                using var data = image.Encode(format, quality);

                var base64 = Convert.ToBase64String(data.ToArray());

                return new ProcessedImageResponse
                {
                    Base64Data = base64,
                    FileSizeBytes = data.Size,
                    Width = processedBitmap.Width,
                    Height = processedBitmap.Height,
                    Format = request.Convert?.TargetFormat ?? "png"
                };
            }
            finally
            {
                if (needsDisposal)
                {
                    processedBitmap.Dispose();
                }
            }
        });
    }

    private SKEncodedImageFormat GetImageFormat(string format)
    {
        return format.ToLower() switch
        {
            "png" => SKEncodedImageFormat.Png,
            "jpg" or "jpeg" => SKEncodedImageFormat.Jpeg,
            "webp" => SKEncodedImageFormat.Webp,
            "bmp" => SKEncodedImageFormat.Bmp,
            "gif" => SKEncodedImageFormat.Gif,
            _ => SKEncodedImageFormat.Png
        };
    }
}
