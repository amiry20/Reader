namespace TestReader.Domain.Entities
{
    public class Alert
    {
        public long Id { get; set; }
        public string RuleId { get; set; } = "";
        public string DeviceId { get; set; } = "";
        public string Metric { get; set; } = "";
        public long StartTsMs { get; set; }
        public long EndTsMs { get; set; }
        public double PeakValue { get; set; }
    }
}
