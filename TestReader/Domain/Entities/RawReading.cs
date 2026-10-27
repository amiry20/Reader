using System.Text.Json.Serialization;

namespace TestReader.Domain.Entities
{
    public class RawReading
    {
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; } = "";

        [JsonPropertyName("metric")]
        public string Metric { get; set; } = "";

        [JsonPropertyName("ts")]
        public string Ts { get; set; } = "";

        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("seq")]
        public int Seq { get; set; }
    }

}
