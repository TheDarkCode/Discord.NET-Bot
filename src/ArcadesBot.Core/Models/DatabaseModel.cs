using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace ArcadesBot
{
    public class DatabaseModel
    {
        public string DatabaseName { get; set; } = "ArcadesBot";
        public string DatabaseUrl { get; set; } = "http://127.0.0.1:8080";

        [JsonProperty("X509CertificatePath")] public string CertificatePath { get; set; }

        [JsonIgnore]
        public X509Certificate2 Certificate =>
            !string.IsNullOrWhiteSpace(CertificatePath) ? new X509Certificate2(CertificatePath) : null;
    }
}