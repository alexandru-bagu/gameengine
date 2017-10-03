using System;
using System.IO;

namespace GameEngine.Assets
{
    public class AssetManager
    {
        public static string RelativeTexture(params string[] pathPieces)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Assets");
            foreach (var piece in pathPieces)
                path = Path.Combine(path, piece);
            return path;
        }

        public static string RelativeAudio(params string[] pathPieces)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Audio");
            foreach (var piece in pathPieces)
                path = Path.Combine(path, piece);
            return path;
        }

        public static string RelativeFont(params string[] pathPieces)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Fonts");
            foreach (var piece in pathPieces)
                path = Path.Combine(path, piece);
            return path;
        }

        public static bool AssetExists(string path)
        {
            return File.Exists(path);
        }
    }
}
