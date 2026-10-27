namespace TestReader.Domain.Models
{
    public class ProcessingReportModel
    {
        public int TotalLinesRead { get; set; }
        public int ParsedReadings { get; set; }
        public long StoredReadings { get; set; }
        public int DuplicatesRemoved { get; set; }
        public int InvalidRecordsRejected { get; set; }
        public int RulesLoaded { get; set; }
        public int RuleEvaluationsPerformed { get; set; }
        public int AcceptableReadings { get; set; }
        public int UnacceptableReadings { get; set; }
        public int RuleViolations { get; set; }
        public int AlertsGenerated { get; set; }
    }
}
