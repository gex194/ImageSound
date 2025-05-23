using System;
using NAudio.Wave;

namespace ImageSound.Services.SoundProcessingService;

public class SoundProcessingService : ISoundProcessingService
{
    public SoundProcessingService() {}
    
    public void GenerateSound(float[] brightnessArray)
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
                GenerateSineWave(buffer, sampleRate, amplitude: 1.5f * brightnessArray[second]);

                // Write the buffer to the WAV file
                WriteBufferToWaveFile(waveFileWriter, buffer);
            }
        }
        
        PlayWavFile(outputFilePath);
    }
    
    public void GenerateSineWave(float[] buffer, int sampleRate, float amplitude, float startFrequency = 440.0f, float endFrequency = 8000.0f)
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
    
    public void PlayWavFile(string filePath)
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