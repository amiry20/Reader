using TestReader.Domain.Entities;

namespace TestReader.Application
{

    public interface IReadingRepository
    {
        Task SaveReadingsAsync(IEnumerable<(RawReading Raw, long TsMs, bool IsAcceptable, string? RuleId, string? Reason)> readings);
        Task SaveAlertsAsync(IEnumerable<Alert> alerts);
        Task<long> CountReadingsAsync();
        Task<IEnumerable<object>> AggregateAsync(string deviceId, string metric, long fromMs, long toMs, long bucketMs);
    }

}
