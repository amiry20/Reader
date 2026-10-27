namespace TestReader.Domain.Entities
{
    public class Rule
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public bool Enabled { get; set; }
        public string Metric { get; set; } = "";
        public string? DeviceId { get; set; }
        public string Operator { get; set; } = "";
        public double? Threshold { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public int? DurationSeconds { get; set; }
    }
}
