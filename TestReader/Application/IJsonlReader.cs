namespace TestReader.Application
{
    public interface IJsonlReader
    {
        IEnumerable<string> ReadLines(string path);
    }
}
