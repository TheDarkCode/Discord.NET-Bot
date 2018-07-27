using System.Collections.Generic;

namespace ArcadesBot
{
    public class ConfigModel
    {
        public string Id { get; set; }
        public string Prefix { get; set; }
        public List<string> Games { get; set; } = new List<string>();
        public List<ulong> Blacklist { get; set; } = new List<ulong>();
        public List<string> Namespaces { get; set; } = new List<string>();
        public Dictionary<string, string> ApiKeys { get; set; }
    }
}