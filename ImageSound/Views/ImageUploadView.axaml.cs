using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ImageSound.ViewModels;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace ImageSound.Views;

public partial class ImageUploadView : UserControl
{
    public ImageUploadView()
    {
        InitializeComponent();
    }
    
    private async void OnUpload(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var filePickerOptions = new FilePickerOpenOptions
        {
            Title = "Choose an image",
            FileTypeFilter = [FilePickerFileTypes.ImageAll],
            AllowMultiple = false
        };
        if (topLevel == null)
        {
            return;
        }
        var file = await topLevel.StorageProvider.OpenFilePickerAsync(filePickerOptions);
        
        if (file is not { Count: > 0 }) return;
        string filePath = file[0].Path.LocalPath;
            
        using var image = Image.Load<Rgba32>(filePath);

        image.Mutate(x => x.Resize(400, 400));
        image.Mutate(x => x.BlackWhite());

        await using var displayStream = File.OpenRead(filePath);
        if (DataContext is SharedViewModel sharedViewModel)
        {
            sharedViewModel.Image = image;
            sharedViewModel.ImagePath = filePath;
            Debug.WriteLine(sharedViewModel.Image.Width, "Image width");
        }

        PreviewImage.Source = new Bitmap(displayStream);
    }
}