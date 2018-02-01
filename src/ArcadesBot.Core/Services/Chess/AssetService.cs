using System.IO;

namespace ArcadesBot
{
    public class AssetService : IAssetService
    {
        public string GetImagePath(string name)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Images", name);
            return path;
        }
    }
}
