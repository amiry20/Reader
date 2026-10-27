using TestReader.Application;
using TestReader.Domain.Entities;
using TestReader.Domain.Models;
using TestReader.Domain.Validation;

namespace TestReader.Domain.Services;

public class IngestionService
{
    private readonly RuleEngine _ruleEngine;
    private readonly SustainedAboveEngine _sustainedEngine;
    private readonly ReadingValidator _validator;
    private readonly IReadingRepository _repository;
    private readonly IJsonlReader _reader;

    public IngestionService(
        RuleEngine ruleEngine,
        SustainedAboveEngine sustainedEngine,
        ReadingValidator validator,
        IReadingRepository repository,
        IJsonlReader reader)
    {
        _ruleEngine = ruleEngine;
        _sustainedEngine = sustainedEngine;
        _validator = validator;
        _repository = repository;
        _reader = reader;
    }

    public async Task<ProcessingReportModel> ProcessFileAsync(string path, List<Rule> rules)
    {
        var report = new ProcessingReportModel();
        var parsed = new List<(RawReading Raw, long TsMs)>();
        var invalid = 0;


        foreach (var line in _reader.ReadLines(path))
        {
            report.TotalLinesRead++;
            if (_validator.TryParse(line, out var raw, out var tsMs))
                parsed.Add((raw, tsMs));
            else
                invalid++;
        }

        report.ParsedReadings = parsed.Count;
        report.InvalidRecordsRejected = invalid;


        var unique = parsed
            .GroupBy(x => (x.Raw.DeviceId, x.Raw.Metric, x.TsMs, x.Raw.Seq))
            .Select(g => g.First())
            .OrderBy(x => x.TsMs)
            .ToList();

        report.DuplicatesRemoved = parsed.Count - unique.Count;


        var results = new List<(RawReading Raw, long TsMs, bool IsAcceptable, string? RuleId, string? Reason)>();

        foreach (var item in unique)
        {
            bool acceptable = true;
            string? ruleId = null, reason = null;

            foreach (var rule in rules)
            {
                var (ok, rid, rsn, evalCount) = _ruleEngine.Evaluate(rule, item.Raw);
                report.RuleEvaluationsPerformed += evalCount;
                if (!ok) { acceptable = false; ruleId = rid; reason = rsn; break; }
            }

            if (acceptable) report.AcceptableReadings++;
            else { report.UnacceptableReadings++; report.RuleViolations++; }

            results.Add((item.Raw, item.TsMs, acceptable, ruleId, reason));
        }


        var alerts = _sustainedEngine.Evaluate(unique, rules);
        report.AlertsGenerated = alerts.Count;


        await _repository.SaveReadingsAsync(results);
        await _repository.SaveAlertsAsync(alerts);

        report.StoredReadings = await _repository.CountReadingsAsync();
        report.RulesLoaded = rules.Count;

        return report;
    }
}