namespace ImageCrop.Web.Models;

public class ProcessedImageResponse
{
    public string Base64Data { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
}

public class ImageInfoResponse
{
    public string ImageId { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string PreviewUrl { get; set; } = string.Empty;
}

public class UploadResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ImageInfoResponse? ImageInfo { get; set; }
}
