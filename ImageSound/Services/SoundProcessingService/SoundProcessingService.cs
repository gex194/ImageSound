using System;
using System.Threading.Tasks;
using ImageSound.ViewModels;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ImageSound.Services.SoundProcessingService;

public class SoundProcessingService : ISoundProcessingService
{
    public WaveOutEvent OutputDevice { get; set; }
    private string OutputFilePath { get; set; }
    private string InputFilePath { get; set; }
    private AudioFileReader _audioFileReader;
    private MediaFoundationReader _mediaFoundationReader;

    public SoundProcessingService()
    {
        InputFilePath = @"D:\Projects\C_sharp\ImageSound\ImageSound\input.wav";
        OutputFilePath = "output.wav";
    }

    public void SetInputFilePath(string inputFilePath)
    {
        InputFilePath = inputFilePath;
    }
    
    public void GenerateSound(float[] brightnessArray)
    {
        StopWavFile();
        
        int sampleRate = 44100;
        int channels = 2;
        int durationSeconds = brightnessArray.Length;
        
        float[] buffer = new float[sampleRate * channels];

        using (var waveFileWriter = new WaveFileWriter(OutputFilePath, new WaveFormat(sampleRate, 16, channels)))
        {
            for (int second = 0; second < durationSeconds; second++)
            {
                float frequency = 200.0f + (brightnessArray[second] * 400f);
                GenerateSineWave(buffer, sampleRate, amplitude: 15.0f * brightnessArray[second], frequency);

                // Write the buffer to the WAV file
                
                WriteBufferToWaveFile(waveFileWriter, buffer);
            }
        }
        
        PlayWavFile(OutputFilePath);
    }

    public async void ModifySound(float[] brightnessArray)
    {
        int durationSeconds = brightnessArray.Length;

        var reader = new MediaFoundationReader(InputFilePath);
        var pitch = new SmbPitchShiftingSampleProvider(reader.ToSampleProvider());
        OutputDevice = new WaveOutEvent();
        var index = 0;

        await Task.Run(() =>
        {
            OutputDevice.Init(pitch);
            OutputDevice.Play();

            while (index < durationSeconds)
            {
                if (reader.CurrentTime >= reader.TotalTime / (Math.Abs(brightnessArray[index]) * 20d))
                {
                    reader.Position = 0;
                    pitch.PitchFactor += brightnessArray[index] * 0.01f;
                }
                
                Task.Delay(100).Wait(); // Small delay to prevent busy-waiting
                index++;
            }

            _mediaFoundationReader = reader;
            
            while (OutputDevice.PlaybackState == PlaybackState.Playing)
            {
                Task.Delay(10).Wait(); // Small delay to prevent busy-waiting
            }
        });
    }
    
    public void GenerateSineWave(float[] buffer, int sampleRate, float amplitude, float startFrequency = 440.0f, float endFrequency = 2000.0f)
    {
        float frequencyStep = (endFrequency - startFrequency) / buffer.Length;
        float currentFrequency = startFrequency;
        
        for (int i = 0; i < buffer.Length; i++)
        {
            float time = i / (float)sampleRate; // Time in seconds
            buffer[i] = amplitude * (float)Math.Sin(2 * Math.PI * startFrequency * time);
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
    
    public async void PlayWavFile(string? filePath)
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
                    Task.Delay(50).Wait(); // Small delay to prevent busy-waiting
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