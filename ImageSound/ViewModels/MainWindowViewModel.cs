using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSound.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public SharedViewModel SharedViewModel { get; set; }

    public MainWindowViewModel()
    {
        SharedViewModel = new SharedViewModel();
    }
}