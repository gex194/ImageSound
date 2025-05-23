using Avalonia.Controls;
using Avalonia.Interactivity;
using ImageSound.Services;
using ImageSound.Services.ImageProcessingService;
using ImageSound.Services.SoundProcessingService;
using ImageSound.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSound.Views;

public partial class SoundControl : UserControl
{
    private Image<Rgba32> image { get; set; }
    private string? imagePath { get; set; }
    private ISoundProcessingService SoundProcessingService { get; set; }
    private IImageProcessingService ImageProcessingService { get; set; }
    public SoundControl()
    {
        InitializeComponent();

        ImageProcessingService = new ImageProcessingService();
        SoundProcessingService = new SoundProcessingService();
    }

    public void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SharedViewModel sharedViewModel)
        {
            image = sharedViewModel.Image;
            imagePath = sharedViewModel.ImagePath;
            var brightnessMatrix = ImageProcessingService.ProcessImage(imagePath);
            var averageBrightnessArray = ImageProcessingService.GetAverageBrightnessArray(brightnessMatrix);
            SoundProcessingService.GenerateSound(averageBrightnessArray.ToArray());
        }
    }
}