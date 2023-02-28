using System.Text.Json.Serialization;

namespace BLAZAM.Server.Data.Services.Duo
{
    public class DuoResponseData
    {
        [JsonPropertyName("response")]
        public DuoResponsee Response { get; set; }

        [JsonPropertyName("stat")]
        public string Stat { get; set; }

    }

    public class DuoDevice
    {
        [JsonPropertyName("capabilities")]
        public List<string> Capabilities { get; set; }

        [JsonPropertyName("device")]
        public string DeviceId { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class DuoResponsee
    {
        [JsonPropertyName("devices")]
        public List<DuoDevice> Devices { get; set; }

        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("status_msg")]
        public string StatusMessage { get; set; }

        [JsonPropertyName("trusted_device_token")]
        public string TustedDeviceToken { get; set; }
    }

}
