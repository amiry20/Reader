using System.Globalization;
using TestReader.Application;

namespace TestReader.Api.Endpoints;

public static class AggregateEndpoint
{
    public static void MapAggregate(this WebApplication app, string cs)
    {
        app.MapGet("/aggregate", async (
            string deviceId, string metric,
            string from, string to, int bucketSeconds,
            IReadingRepository repo) =>
        {
            if (bucketSeconds <= 0) return Results.BadRequest("bucketSeconds must be > 0");

            if (!DateTime.TryParse(from, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var f)
                || !DateTime.TryParse(to, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var t))
                return Results.BadRequest("Invalid from/to");

            if (f >= t) return Results.BadRequest("from must be before to");

            var fromMs = new DateTimeOffset(f, TimeSpan.Zero).ToUnixTimeMilliseconds();
            var toMs = new DateTimeOffset(t, TimeSpan.Zero).ToUnixTimeMilliseconds();
            var bucketMs = bucketSeconds * 1000L;

            var result = await repo.AggregateAsync(deviceId, metric, fromMs, toMs, bucketMs);
            return Results.Ok(result);
        });
    }
}