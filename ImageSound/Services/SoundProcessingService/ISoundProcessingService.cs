using NAudio.Wave;

namespace ImageSound.Services.SoundProcessingService;

public interface ISoundProcessingService
{
    public void GenerateSineWave(float[] buffer, int sampleRate, float amplitude, float startFrequency = 440.0f,
        float endFrequency = 8000.0f);

    public void WriteBufferToWaveFile(WaveFileWriter writer, float[] buffer);
    public void PlayWavFile(string filePath);
    public void GenerateSound(float[] brightnessArray);
}