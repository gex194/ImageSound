using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ImageSound.ViewModels;
using NAudio.Wave;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace ImageSound.Views;

public partial class SoundControl : UserControl
{
    private Image<Rgba32> image { get; set; }
    private string? imagePath { get; set; }
    public SoundControl()
    {
        InitializeComponent();
    }

    public void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SharedViewModel sharedViewModel)
        {
            image = sharedViewModel.Image;
            imagePath = sharedViewModel.ImagePath;
            var brightnessMatrix = ProcessImage(imagePath);
            var averageBrightnessArray = GetAverageBrightnessArray(brightnessMatrix);
            GenerateSound(averageBrightnessArray.ToArray());
        }
    }

    private float[,] ProcessImage(string imagePath)
    {
        using Image<Rgba32> loadedImage = Image.Load<Rgba32>(imagePath);
        float [,] brightnessMatrix = new float[loadedImage.Width, loadedImage.Height];
        loadedImage.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                for (int x = 0; x < accessor.Width; x++)
                {
                    ref Rgba32 pixel = ref pixelRow[x];
                    
                    float brightness = 0.2126f * pixel.R + 0.7152f * pixel.G + 0.0722f * pixel.B;
                    brightnessMatrix[x, y] = brightness / 255f;
                }
            }
        });
        Debug.WriteLine("Brightness:", brightnessMatrix);
        var rotatedMatrix = AlignBrightnessMatrix(brightnessMatrix);
        var result = FlipMatrixTopAndBottom(rotatedMatrix);
        return result;
    }

    private float[,] AlignBrightnessMatrix(float[,] brightnessMatrix)
    {
        int rows = brightnessMatrix.GetLength(0);
        int columns = brightnessMatrix.GetLength(1);
        float[,] alignedBrightnessMatrix = new float[columns, rows];
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                alignedBrightnessMatrix[columns - 1 - col,row] = brightnessMatrix[row, col];
            }
        }

        return alignedBrightnessMatrix;
    }
    
    private float[,] FlipMatrixTopAndBottom(float[,] brightnessMatrix)
    {
        int rows = brightnessMatrix.GetLength(0);
        int columns = brightnessMatrix.GetLength(1);
        float[,] alignedBrightnessMatrix = new float[rows, columns];
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                alignedBrightnessMatrix[rows - 1 - row,col] = brightnessMatrix[row, col];
            }
        }

        return alignedBrightnessMatrix;
    }

    private List<float> GetAverageBrightnessArray(float[,] brightnessMatrix)
    {
        List<float> averageBrightnessArray = new List<float>();
        for (int y = 0; y < brightnessMatrix.GetLength(1); y++)
        {
            float ySum = 0.0f;
            for (int x = 0; x < brightnessMatrix.GetLength(0); x++)
            {
                ySum += brightnessMatrix[x, y];
            }
            float averageBrightness = ySum / brightnessMatrix.GetLength(0);
            if (averageBrightness < 0.5f)
            {
                averageBrightness = -averageBrightness;
            }
            averageBrightnessArray.Add(averageBrightness);
        }
        
        return averageBrightnessArray;
    }

    private void GenerateSound(float[] brightnessArray)
    {
        string outputFilePath = "output.wav";

        int sampleRate = 44100;
        int channels = 1;
        int durationSeconds = 10;

        float[] buffer = new float[sampleRate * channels];

        using (var waveFileWriter = new WaveFileWriter(outputFilePath, new WaveFormat(sampleRate, 16, channels)))
        {
            for (int second = 0; second < durationSeconds; second++)
            {
                float frequency = 440.0f + (brightnessArray[second] * 500f);
                GenerateSineWave(buffer, sampleRate, frequency, amplitude: 1.5f * brightnessArray[second]);

                // Write the buffer to the WAV file
                WriteBufferToWaveFile(waveFileWriter, buffer);
            }
        }
        
        PlayWavFile(outputFilePath);
    }

    static void GenerateSineWave(float[] buffer, int sampleRate, float frequency, float amplitude, float startFrequency = 440.0f, float endFrequency = 10880.0f)
    {
        float frequencyStep = (endFrequency - startFrequency) / buffer.Length;

        float currentFrequency = startFrequency;
        for (int i = 0; i < buffer.Length; i++)
        {
            float time = i / (float)sampleRate; // Time in seconds
            buffer[i] = amplitude * (float)Math.Sin(2 * Math.PI * frequency * time);
            currentFrequency += frequencyStep;
        }
    }
    
    static void WriteBufferToWaveFile(WaveFileWriter writer, float[] buffer)
    {
        foreach (var sample in buffer)
        {
            // Clip to [-1, 1] range
            float clippedSample = Math.Max(-1.0f, Math.Min(1.0f, sample));

            // Scale to 16-bit range and write
            short pcmSample = (short)(clippedSample * short.MaxValue);
            writer.WriteSample(pcmSample);
        }
    }
    
    static void PlayWavFile(string filePath)
    {
        using (var audioFileReader = new AudioFileReader(filePath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(audioFileReader);
            outputDevice.Volume = 0.2f;
            outputDevice.Play();

            // Wait for playback to finish
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}