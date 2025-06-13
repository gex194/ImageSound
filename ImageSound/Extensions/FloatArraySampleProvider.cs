using System;
using NAudio.Wave;

namespace ImageSound.Extensions;

public class FloatArraySampleProvider : ISampleProvider
{
    private readonly float[] _samples;
    private int _position;
    private readonly WaveFormat _waveFormat;

    public FloatArraySampleProvider(float[] samples, int channels)
    {
        _samples = samples;
        _position = 0;
        _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, channels);
    }

    public WaveFormat WaveFormat => _waveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int availableSamples = Math.Min(count, _samples.Length - _position);
        Array.Copy(_samples, _position, buffer, offset, availableSamples);
        _position += availableSamples;
        return availableSamples;
    }
}