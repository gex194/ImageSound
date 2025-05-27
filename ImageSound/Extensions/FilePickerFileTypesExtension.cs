using Avalonia.Platform.Storage;

namespace ImageSound.Extensions;

public class FilePickerFileTypesExtension
{
    public static FilePickerFileType SoundWav { get; } = new("All Sounds")
    {
        Patterns = new[] { "*.wav" },
        AppleUniformTypeIdentifiers = new[] { "public.wav" },
        MimeTypes = new[] { "sound/*" }
    };
}