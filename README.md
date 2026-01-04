# ImageAdjust - ASP.NET Core Image Processing Library

[![NuGet](https://img.shields.io/badge/nuget-v1.0.0-blue)](https://www.nuget.org/packages/ahmad.ImageAdjust/)
[![.NET Core](https://img.shields.io/badge/.NET%20Core-8.0%2B-green)](https://dotnet.microsoft.com/)

**ImageAdjust** is a powerful and easy-to-integrate Razor Class Library (RCL) for ASP.NET Core applications. It provides a complete UI and backend service for image cropping, resizing, format conversion (JPG, PNG, WebP, BMP), and compression.

<img width="1920" height="912" alt="demo-screenshot" src="https://github.com/user-attachments/assets/d70d668a-ac9d-46b9-b529-9ee9865c13b8" />


## Features

*   **✂️ Interactive Cropping**: Built-in drag-and-drop UI powered by Cropper.js.
*   **📐 Resizing**: Resize images with or without aspect ratio locking.
*   **🔄 Format Conversion**: Convert images between PNG, JPG, WebP, and BMP.
*   **🗜️ Compression**: Adjust image quality to reduce file size.
*   **🎨 Pre-built UI**: Ready-to-use Razor Pages and styles (Glassmorphism design).
*   **🔌 Easy Integration**: minimal setup required in your host application.

## Installation

Install the package via NuGet Package Manager Console:

`powershell
Install-Package ahmad.ImageAdjust
`

Or via the .NET CLI:

`dotnet
add package ahmad.ImageAdjust
`

## Getting Started

### 1. Register Services
In your host application's Program.cs, add the following line to register the library's services and Razor pages:

`csharp
using ImageCrop.Web; // Add this namespace

var builder = WebApplication.CreateBuilder(args);

// ... other services

// Add ImageAdjust services
builder.Services.AddImageCrop(); 

var app = builder.Build();
`

### 2. Map Static Assets
Keep your standard static file mapping:

`csharp
app.UseStaticFiles();
app.MapRazorPages();
`

### 3. Usage
The library provides its own UI at the /ImageCrop route. You can link to it from your application's navigation:

`html
<a href="/ImageCrop">Open Image Editor</a>
`

Or redirect your home page to it in Pages/Index.cshtml.cs:

`csharp
public IActionResult OnGet()
{
    return Redirect("/ImageCrop");
}
`

## Configuration
The library uses SkiaSharp for high-performance image processing. No additional system setup is usually required for Windows/Linux environments.

## Development

To run the project locally for development:

1.  Clone the repository.
2.  Open ImageCrop.sln.
3.  Set ImageCropWebsite as the startup project.
4.  Run the application.

## License

MIT License
