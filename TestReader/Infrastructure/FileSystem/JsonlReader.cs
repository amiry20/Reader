using TestReader.Application;

namespace TestReader.Infrastructure.FileSystem;

public class JsonlReader : IJsonlReader
{
    public IEnumerable<string> ReadLines(string path) => File.ReadLines(path);
}