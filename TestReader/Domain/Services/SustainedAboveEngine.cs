using TestReader.Domain.Entities;

namespace TestReader.Domain.Services;

public class SustainedAboveEngine
{

    private const long CooldownMs = 5 * 60 * 1000L;


    public List<Alert> Evaluate(
        List<(RawReading Raw, long TsMs)> sortedReadings,
        List<Rule> rules)
    {
        var allAlerts = new List<Alert>();

        var sustainedRules = rules
            .Where(r => r.Enabled && r.Operator == "SustainedAbove")
            .ToList();

        foreach (var rule in sustainedRules)
        {

            if (!rule.Threshold.HasValue || !rule.DurationSeconds.HasValue)
                continue;

            var durationMs = rule.DurationSeconds.Value * 1000L;


            var groups = sortedReadings
                .Where(x => string.Equals(x.Raw.Metric, rule.Metric,
                                          StringComparison.OrdinalIgnoreCase)
                            && (string.IsNullOrEmpty(rule.DeviceId)
                                || string.Equals(x.Raw.DeviceId, rule.DeviceId,
                                                 StringComparison.OrdinalIgnoreCase)))
                .GroupBy(x => x.Raw.DeviceId);

            foreach (var group in groups)
            {
                var deviceId = group.Key;
                long? episodeStart = null;
                double peak = 0;

                foreach (var item in group)
                {
                    if (item.Raw.Value > rule.Threshold.Value)
                    {
                        episodeStart ??= item.TsMs;
                        peak = Math.Max(peak, item.Raw.Value);
                    }
                    else if (episodeStart.HasValue)
                    {
                        var duration = item.TsMs - episodeStart.Value;

                        if (duration >= durationMs)
                        {
                            allAlerts.Add(new Alert
                            {
                                RuleId = rule.Id,
                                DeviceId = deviceId,
                                Metric = rule.Metric,
                                StartTsMs = episodeStart.Value,
                                EndTsMs = item.TsMs,
                                PeakValue = peak
                            });
                        }

                        episodeStart = null;
                        peak = 0;
                    }
                }

                if (episodeStart.HasValue)
                {
                    var lastTs = group.Max(x => x.TsMs);
                    var duration = lastTs - episodeStart.Value;

                    if (duration >= durationMs)
                    {
                        allAlerts.Add(new Alert
                        {
                            RuleId = rule.Id,
                            DeviceId = deviceId,
                            Metric = rule.Metric,
                            StartTsMs = episodeStart.Value,
                            EndTsMs = lastTs,
                            PeakValue = peak
                        });
                    }
                }
            }
        }

        return ApplyCooldown(allAlerts);
    }

    public static List<Alert> ApplyCooldown(List<Alert> alerts)
    {
        var result = new List<Alert>();

        foreach (var group in alerts
                     .GroupBy(a => (a.RuleId, a.DeviceId, a.Metric)))
        {
            Alert? lastKept = null;

            foreach (var alert in group.OrderBy(a => a.StartTsMs))
            {
                if (lastKept == null
                    || alert.StartTsMs - lastKept.StartTsMs >= CooldownMs)
                {
                    result.Add(alert);
                    lastKept = alert;
                }
            }
        }

        return result;
    }
}