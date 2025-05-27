using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ImageSound.Enums;
using ImageSound.Extensions;
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
            switch (SoundType.SelectedIndex)
            {
                case (int)SoundTypes.GenerateSound:
                    SoundProcessingService.GenerateSound(averageBrightnessArray.ToArray());
                    break;
                case (int)SoundTypes.ModifySound:
                    SoundProcessingService.ModifySound(averageBrightnessArray.ToArray());
                    break;
            }
        }
    }

    public async void OnWavFileUploadButtonClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var filePickerOptions = new FilePickerOpenOptions
        {
            Title = "Choose a .wav file",
            FileTypeFilter = [FilePickerFileTypesExtension.SoundWav],
            AllowMultiple = false
        };

        if (topLevel == null)
        {
            return;
        }
        
        var file = await topLevel?.StorageProvider?.OpenFilePickerAsync(filePickerOptions);
        if (DataContext is SharedViewModel sharedViewModel)
        {
            sharedViewModel.BrightnessWavFilePath = file[0].Path.LocalPath;
            WavInputFile.Content = file[0].Name;
            SoundProcessingService.SetInputFilePath(file[0].Path.LocalPath);
        }
    }

    public void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (WavInputFile != null && SoundType != null)
        {
            WavInputFile.IsVisible = SoundType.SelectedIndex == (int)SoundTypes.ModifySound;
        }
    }

    public void OnSoundStop(object? sender, RoutedEventArgs e)
    {
        SoundProcessingService.StopWavFile();
        PlayButton.IsEnabled = true;
    }
}