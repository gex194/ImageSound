using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ImageSound.Services.SoundProcessingService;

public interface ISoundProcessingService
{
    public WaveOutEvent OutputDevice { get; set; }
    public void GenerateSineWave(float[] buffer, int sampleRate, float amplitude, float startFrequency = 440.0f,
        float endFrequency = 8000.0f);

    public void WriteBufferToWaveFile(WaveFileWriter writer, float[] buffer);
    public void SetInputFilePath(string inputFilePath);
    public void PlayWavFile(string? outputFilePath);
    public void GenerateSound(float[] brightnessArray);
    public void ModifySound(float[] brightnessArray);
    public void StopWavFile();
}