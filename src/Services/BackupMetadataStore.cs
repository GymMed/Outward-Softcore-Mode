using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OutwardSoftcoreMode.Services
{
    public static class BackupMetadataStore
    {
        private const string FILENAME = "SoftcoreSaveData.xml";

        private static string GetFilePath(string uid, string instanceTimestamp) =>
            Path.Combine(SoftcorePaths.GetBackupsDir(uid), instanceTimestamp, FILENAME);

        public static void WriteGameTime(string uid, string instanceTimestamp, float gameTime)
        {
            string path = GetFilePath(uid, instanceTimestamp);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var doc = new XDocument(
                new XElement("SoftcoreBackupData",
                    new XElement("GameTime", gameTime.ToString("F2"))
                )
            );
            doc.Save(path);
        }

        private static bool TryReadGameTime(string uid, string instanceTimestamp, out float gameTime)
        {
            string path = GetFilePath(uid, instanceTimestamp);
            if (!File.Exists(path))
            {
                gameTime = -1f;
                return false;
            }
            try
            {
                var doc = XDocument.Load(path);
                var el = doc.Root?.Element("GameTime");
                if (el != null && float.TryParse(el.Value, out gameTime))
                    return true;
            }
            catch { }
            gameTime = -1f;
            return false;
        }

        public static float GetLatestBackupGameTime(string uid)
        {
            string backupsDir = SoftcorePaths.GetBackupsDir(uid);
            if (!Directory.Exists(backupsDir))
                return -1f;

            var dirs = new DirectoryInfo(backupsDir)
                .GetDirectories()
                .OrderByDescending(d => d.Name);

            foreach (var dir in dirs)
            {
                if (TryReadGameTime(uid, dir.Name, out float gameTime))
                    return gameTime;
            }

            return -1f;
        }
    }
}
