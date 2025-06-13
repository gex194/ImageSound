using System;
using System.Threading.Tasks;
using ImageSound.Extensions;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ImageSound.Services.SoundProcessingService;

public class SoundProcessingService : ISoundProcessingService
{
    public WaveOutEvent? OutputDevice { get; set; }
    private string OutputFilePath { get; set; }
    private string InputFilePath { get; set; }
    private AudioFileReader? _audioFileReader;

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
    
public async void ModifySound(float[] brightnessArray, int totalDurationSeconds, float segmentLengthSeconds)
{
    StopWavFile();

    using var reader = new WaveFileReader(InputFilePath);
    var sampleProvider = reader.ToSampleProvider();
    OutputDevice = new WaveOutEvent();
    
    // Calculate buffer sizes
    int sampleRate = sampleProvider.WaveFormat.SampleRate;
    int channels = sampleProvider.WaveFormat.Channels;
    int totalSamplesNeeded = sampleRate * channels * totalDurationSeconds;
    int segmentSamples = (int)(sampleRate * channels * segmentLengthSeconds);
    
    // Create buffers
    var outputBuffer = new float[totalSamplesNeeded];
    var segmentBuffer = new float[segmentSamples];
    
    OutputFilePath = DateTime.Now.ToString("yyyy_MM_dd_HHmmss") + "_modify_output.wav";
    
    await Task.Run(() =>
    {
        using var writer = new WaveFileWriter(OutputFilePath, sampleProvider.WaveFormat);

        // Read the segment we'll be repeating
        int samplesRead = sampleProvider.Read(segmentBuffer, 0, segmentSamples);
        
        if (samplesRead > 0)
        {
            // If we read less than requested, adjust segment size
            if (samplesRead < segmentSamples)
            {
                segmentSamples = samplesRead;
                Array.Resize(ref segmentBuffer, segmentSamples);
            }

            // Fill the output buffer by repeating the segment
            int position = 0;
            while (position < totalSamplesNeeded)
            {
                int samplesToWrite = Math.Min(segmentSamples, totalSamplesNeeded - position);
                Array.Copy(segmentBuffer, 0, outputBuffer, position, samplesToWrite);
                position += samplesToWrite;
            }

            // Process the entire buffer with pitch changes
            var processedBuffer = new float[totalSamplesNeeded];
            Array.Copy(outputBuffer, processedBuffer, totalSamplesNeeded);

            // Apply pitch changes for each second
            for (int second = 0; second < totalDurationSeconds && second < brightnessArray.Length; second++)
            {
                int startSample = second * sampleRate * channels;
                int endSample = Math.Min((second + 1) * sampleRate * channels, totalSamplesNeeded);
                float pitchFactor = 0.25f + (brightnessArray[second] * 3.75f);

                var floatProvider = new FloatArraySampleProvider(
                    outputBuffer, sampleProvider.WaveFormat.Channels);
                var pitchShifter = new SmbPitchShiftingSampleProvider(floatProvider);
                pitchShifter.PitchFactor = pitchFactor;

                pitchShifter.Read(processedBuffer, startSample, endSample - startSample);
            }

            writer.WriteSamples(processedBuffer, 0, totalSamplesNeeded);
        }
        
        PlayWavFile(OutputFilePath);
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
            float clippedSample = sample;
            
            // Scale to 16-bit range and write
            short pcmSample = (short)(clippedSample * short.MaxValue);
            if (pcmSample > 0)
            {
                writer.WriteSample(sample);
            }
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