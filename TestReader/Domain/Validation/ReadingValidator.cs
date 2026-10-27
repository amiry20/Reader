using System.Globalization;
using System.Text.Json;
using TestReader.Domain.Entities;

namespace TestReader.Domain.Validation;

public class ReadingValidator
{
    private static readonly Dictionary<string, (double Min, double Max)> MetricBounds = new()
    {
        ["temperature"] = (-50, 200),
        ["pressure"] = (-10, 100),
        ["vibration"] = (0, 50),
    };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public bool TryParse(string line, out RawReading raw, out long tsMs)
    {
        raw = null!;
        tsMs = 0;

        if (string.IsNullOrWhiteSpace(line))
            return false;

        RawReading? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<RawReading>(line, JsonOpts);
        }
        catch (JsonException)
        {
            return false;
        }

        if (parsed == null
            || string.IsNullOrEmpty(parsed.DeviceId)
            || string.IsNullOrEmpty(parsed.Metric))
            return false;

        if (!TryParseTimestamp(parsed.Ts, out tsMs))
            return false;

        var metricKey = parsed.Metric.ToLowerInvariant();
        if (!MetricBounds.TryGetValue(metricKey, out var bounds)
            || parsed.Value < bounds.Min
            || parsed.Value > bounds.Max)
            return false;

        raw = parsed;
        return true;
    }


    public static bool TryParseTimestamp(string raw, out long tsMs)
    {
        tsMs = 0;

        if (!DateTimeOffset.TryParse(
                raw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dto))
            return false;

        tsMs = dto.ToUnixTimeMilliseconds();
        return true;
    }
}