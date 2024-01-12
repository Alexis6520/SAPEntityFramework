using System.Text.Json.Serialization;

namespace SAPEntityFramework
{
    internal class SLSession
    {
        public string SessionId { get; set; }
        public int SessionTimeout { get; set; }

        [JsonIgnore]
        public DateTime LastLogin { get; set; }

        [JsonIgnore]
        public bool IsExpired { get => LastLogin.AddMinutes(SessionTimeout) < DateTime.Now; }
    }
}
