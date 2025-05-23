using System.Collections.Generic;

namespace ImageSound.Services;

public interface IImageProcessingService
{
    public float[,] ProcessImage(string imagePath);
    public List<float> GetAverageBrightnessArray(float[,] brightnessMatrix);
}