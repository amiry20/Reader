using Dapper;
using Microsoft.Data.Sqlite;
using TestReader.Application;
using TestReader.Domain.Entities;

namespace TestReader.Infrastructure.Persistence;

public class SqliteReadingRepository : IReadingRepository
{
    private readonly string _cs;
    public SqliteReadingRepository(string cs) => _cs = cs;

    public async Task SaveReadingsAsync(IEnumerable<(RawReading Raw, long TsMs, bool IsAcceptable, string? RuleId, string? Reason)> readings)
    {
        using var conn = new SqliteConnection(_cs);
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        foreach (var r in readings)
        {
            await conn.ExecuteAsync(@"
                INSERT OR IGNORE INTO Readings
                (DeviceId, Metric, TsMs, Value, Seq, IsAcceptable, ViolatedRuleId, ViolationReason)
                VALUES (@DeviceId, @Metric, @TsMs, @Value, @Seq, @IsAcceptable, @ViolatedRuleId, @ViolationReason)",
                new
                {
                    r.Raw.DeviceId,
                    r.Raw.Metric,
                    r.TsMs,
                    r.Raw.Value,
                    r.Raw.Seq,
                    IsAcceptable = r.IsAcceptable ? 1 : 0,
                    ViolatedRuleId = r.RuleId,
                    ViolationReason = r.Reason
                },
                tx);
        }

        tx.Commit();
    }

    public async Task SaveAlertsAsync(IEnumerable<Alert> alerts)
    {
        using var conn = new SqliteConnection(_cs);
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();

        foreach (var a in alerts)
        {
            await conn.ExecuteAsync(@"
                INSERT OR IGNORE INTO Alerts
                (RuleId, DeviceId, Metric, StartTsMs, EndTsMs, PeakValue)
                VALUES (@RuleId, @DeviceId, @Metric, @StartTsMs, @EndTsMs, @PeakValue)",
                a, tx);
        }

        tx.Commit();
    }

    public async Task<long> CountReadingsAsync()
    {
        using var conn = new SqliteConnection(_cs);
        await conn.OpenAsync();
        return await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Readings");
    }

    public async Task<IEnumerable<object>> AggregateAsync(string deviceId, string metric, long fromMs, long toMs, long bucketMs)
    {
        using var conn = new SqliteConnection(_cs);
        await conn.OpenAsync();

        var rows = await conn.QueryAsync(@"
            SELECT (TsMs - @fromMs) / @bucketMs AS BucketIndex,
                   COUNT(*) AS Count, AVG(Value) AS Average,
                   MIN(Value) AS Min, MAX(Value) AS Max
            FROM Readings
            WHERE DeviceId = @deviceId AND Metric = @metric
              AND IsAcceptable = 1
              AND TsMs >= @fromMs AND TsMs < @toMs
            GROUP BY BucketIndex ORDER BY BucketIndex",
            new { deviceId, metric, fromMs, toMs, bucketMs });

        return rows.Select(r => new
        {
            bucketStart = DateTimeOffset.FromUnixTimeMilliseconds(
                fromMs + (long)r.BucketIndex * bucketMs).UtcDateTime,
            count = (long)r.Count,
            average = Math.Round((double)r.Average, 4),
            min = (double)r.Min,
            max = (double)r.Max
        }).ToList();
    }
}