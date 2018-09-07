using System.IO;
using System.Reflection;

namespace ArcadesBot.Utility
{
    public static class AppHelper
    {
        /// <summary>
        /// Some general utility for the Discord bot 
        /// </summary>
        public static string Version { get; } =
            typeof(AppHelper).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ??
            typeof(AppHelper).GetTypeInfo().Assembly.GetName().Version.ToString(3) ??
            "Unknown";

        /// <summary>
        /// Creates directory with the name of parameter directory
        /// </summary>
        /// <param name="directory">The name of the directory</param>
        public static void CreateDirectory(string directory)
        {
            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}\\{directory}\\"))
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\{directory}\\");
        }
    }
}