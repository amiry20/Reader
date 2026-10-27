namespace TestReader.Domain.Entities
{
    public class ProcessedReading
    {
        public long Id { get; set; }
        public string DeviceId { get; set; } = "";
        public string Metric { get; set; } = "";
        public long TsMs { get; set; }
        public double Value { get; set; }
        public int Seq { get; set; }
        public bool IsAcceptable { get; set; }
        public string? ViolatedRuleId { get; set; }
        public string? ViolationReason { get; set; }
    }
}
