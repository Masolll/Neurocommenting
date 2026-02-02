using Neurocommenting.Infrastructure;

namespace Neurocommenting.Channels;

public static class ChannelsRepository
{
    private static FileStream? fileStream;
    private static StreamReader? streamReader;
    private static object lockObject = new object();

    public static void OpenFile()
    {
        if (streamReader != null)
        {
            throw new Exception("Попытка повторно открыть файл с каналами.");
        }
        fileStream = new FileStream(AppPaths.ChannelsFile, FileMode.Open);
        streamReader = new StreamReader(fileStream);
    }

    public static string? GetNextChannel()
    {
        lock (lockObject)
        {
            if (streamReader is null)
            {
                throw new Exception("Перед использованием GetNextChannel откройте файл с помощью метода OpenFile.");
            }
            return streamReader.ReadLine();
        }
    }

    public static void DisposeResources()
    {
        if (streamReader is null)
        {
            throw new Exception("Невозможно вызывать метод DisposeResources без открытия файла методом OpenFile");
        }
        streamReader?.Dispose();
        fileStream = null;
        streamReader = null;
    }

    public static string ConvertLinkToUsername(string channelLink)
    {
        if (channelLink.StartsWith("https://t.me/"))
        {
            return String.Concat(channelLink.Skip(13));
        }
        else if (channelLink.StartsWith('@'))
        {
            return String.Concat(channelLink.Skip(1));
        }
        else
        {
            throw new ArgumentException($"Некорректная ссылка на канал {channelLink}");
        }
    }
}
