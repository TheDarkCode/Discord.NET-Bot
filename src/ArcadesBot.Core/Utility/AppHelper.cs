using System.IO;
using System.Reflection;

namespace ArcadesBot
{
    public static class AppHelper
    {
        public static string Version { get; } =
            typeof(AppHelper).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ??
            typeof(AppHelper).GetTypeInfo().Assembly.GetName().Version.ToString(3) ??
            "Unknown";

        public static void CreateDirectory(string directory)
        {
            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}\\{directory}\\"))
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\{directory}\\");
        }
    }
}