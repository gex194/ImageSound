using Avalonia.Controls;
using ImageSound.ViewModels;

namespace ImageSound.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}