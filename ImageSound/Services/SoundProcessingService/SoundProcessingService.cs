using System;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ImageSound.Services.SoundProcessingService;

public class SoundProcessingService : ISoundProcessingService
{
    private string OutputFilePath { get; set; }
    private string InputFilePath { get; set; }
    public WaveOutEvent OutputDevice { get; set; }
    private AudioFileReader _audioFileReader;

    public SoundProcessingService()
    {
        InputFilePath = "input.wav";
        OutputFilePath = "output.wav";
    }
    
    public void GenerateSound(float[] brightnessArray)
    {
        StopWavFile();
        
        int sampleRate = 44100;
        int channels = 2;
        int durationSeconds = 10;
        float pitchFactor = 1.2f;
        
        float[] buffer = new float[sampleRate * channels];

        using (var waveFileWriter = new WaveFileWriter(OutputFilePath, new WaveFormat(sampleRate, 16, channels)))
        {
            for (int second = 0; second < durationSeconds; second++)
            {
                float frequency = 100.0f + (brightnessArray[second] * 500f);
                pitchFactor *= brightnessArray[second];
                GenerateSineWave(buffer, sampleRate, amplitude: 5.0f * brightnessArray[second], frequency);

                // Write the buffer to the WAV file
                
                WriteBufferToWaveFile(waveFileWriter, buffer);
            }
        }
        
        PlayWavFile(OutputFilePath);
    }
    
    public void GenerateSineWave(float[] buffer, int sampleRate, float amplitude, float startFrequency = 440.0f, float endFrequency = 2000.0f)
    {
        float frequencyStep = (endFrequency - startFrequency) / buffer.Length;
        float currentFrequency = startFrequency;
        
        for (int i = 0; i < buffer.Length; i++)
        {
            float time = i / (float)sampleRate; // Time in seconds
            buffer[i] = amplitude * (float)Math.Sin(2 * Math.PI * currentFrequency * time);
            currentFrequency += frequencyStep;
        }
    }
    
    public void WriteBufferToWaveFile(WaveFileWriter writer, float[] buffer)
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
    
    public async void PlayWavFile(string filePath)
    {
        await Task.Run(() =>
        {
            try
            {
                _audioFileReader = new AudioFileReader(filePath);
                OutputDevice = new WaveOutEvent();
                OutputDevice.Init(_audioFileReader);
                OutputDevice.Volume = 0.2f;
                OutputDevice.Play();
                while (OutputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Task.Delay(100).Wait(); // Small delay to prevent busy-waiting
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
    }

    public void StopWavFile()
    {
        OutputDevice?.Stop();
        OutputDevice?.Dispose();
        _audioFileReader?.Dispose();
    }
}