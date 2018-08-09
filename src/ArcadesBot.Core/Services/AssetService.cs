using System.IO;

namespace ArcadesBot
{
    public class AssetService
    {
        public string GetImagePath(string directory, string name)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Images", directory, name);
            return path;
        }
    }
}