namespace ImageCrop.Web.Models;

public class CropRequest
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class ResizeRequest
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public bool MaintainAspectRatio { get; set; } = true;
}

public class ConvertRequest
{
    public string TargetFormat { get; set; } = "png";
    public int Quality { get; set; } = 90;
}

public class CompressRequest
{
    public int Quality { get; set; } = 80;
}

public class ProcessImageRequest
{
    public string ImageId { get; set; } = string.Empty;
    public CropRequest? Crop { get; set; }
    public ResizeRequest? Resize { get; set; }
    public ConvertRequest? Convert { get; set; }
    public CompressRequest? Compress { get; set; }
}
