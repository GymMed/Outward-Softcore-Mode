using System.IO;

namespace OutwardSoftcoreMode.Services
{
    public static class MarkerFileService
    {
        public static string GetMarkerPath(string uid) =>
            Path.Combine(SoftcorePaths.RootPath, $"{uid}.restored");

        public static void WriteRestoredMarker(string uid)
        {
            string path = GetMarkerPath(uid);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, string.Empty);
        }

        public static bool HasRestoredMarker(string uid) =>
            File.Exists(GetMarkerPath(uid));

        public static void DeleteRestoredMarker(string uid)
        {
            string path = GetMarkerPath(uid);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
