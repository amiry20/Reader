using TestReader.Domain.Entities;

namespace TestReader.Domain.Services;

public class RuleEngine
{
    public (bool IsAcceptable, string? RuleId, string? Reason, int EvalCount)
        Evaluate(Rule rule, RawReading raw)
    {
        if (!rule.Enabled) return (true, null, null, 0);
        if (!Applies(rule, raw)) return (true, null, null, 0);
        if (rule.Operator == "SustainedAbove") return (true, null, null, 0);

        var ok = CheckStatelessRule(rule, raw);
        return ok
            ? (true, null, null, 1)
            : (false, rule.Id, $"Rule '{rule.Name}' violated", 1);
    }

    private static bool Applies(Rule rule, RawReading raw)
        => string.Equals(rule.Metric, raw.Metric, StringComparison.OrdinalIgnoreCase)
           && (string.IsNullOrEmpty(rule.DeviceId)
               || string.Equals(rule.DeviceId, raw.DeviceId, StringComparison.OrdinalIgnoreCase));

    private static bool CheckStatelessRule(Rule rule, RawReading raw)
        => rule.Operator switch
        {
            "GreaterThan" => raw.Value > rule.Threshold,
            "GreaterThanOrEqual" => raw.Value >= rule.Threshold,
            "LessThan" => raw.Value < rule.Threshold,
            "LessThanOrEqual" => raw.Value <= rule.Threshold,
            "Equal" => rule.Threshold.HasValue
                                    && Math.Abs(raw.Value - rule.Threshold.Value) < 1e-9,
            "Between" => rule.Min.HasValue && rule.Max.HasValue
                                    && raw.Value >= rule.Min.Value
                                    && raw.Value <= rule.Max.Value,
            _ => true
        };
}