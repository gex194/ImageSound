using System;
using System.Collections.Generic;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSound.Services.ImageProcessingService;

public class ImageProcessingService : IImageProcessingService
{
    public ImageProcessingService() {}
    
    public float[,] ProcessImage(string imagePath)
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

    public List<float> GetAverageBrightnessArray(float[,] brightnessMatrix)
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
}