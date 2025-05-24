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
    private Image<Rgba32> Image { get; set; }
    private string? ImagePath { get; set; }
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
            if (sharedViewModel.ImagePath == null)
            {
                return;
            }
            Image = sharedViewModel.Image;
            ImagePath = sharedViewModel.ImagePath;
            var brightnessMatrix = ImageProcessingService.ProcessImage(ImagePath);
            var averageBrightnessArray = ImageProcessingService.GetAverageBrightnessArray(brightnessMatrix);
            // SoundProcessingService.GenerateSound(averageBrightnessArray.ToArray());
            SoundProcessingService.ModifySound(averageBrightnessArray.ToArray());
        }
    }

    public void OnSoundStop(object? sender, RoutedEventArgs e)
    {
        SoundProcessingService.StopWavFile();
        PlayButton.IsEnabled = true;
    }
}