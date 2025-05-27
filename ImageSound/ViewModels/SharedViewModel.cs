using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSound.ViewModels;

public class SharedViewModel : ViewModelBase
{
    public Image<Rgba32> Image { get; set; }
    public string? ImagePath { get; set; }
    public string? BrightnessWavFilePath { get; set; }
}