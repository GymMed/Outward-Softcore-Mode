using OutwardModsCommunicator.Managers;
using System.IO;

namespace OutwardSoftcoreMode.Services
{
    public static class SoftcorePaths
    {
        public static string RootPath =>
            Path.Combine(PathsManager.ConfigPath, "Softcore_Mode");

        public static string CharactersPath =>
            Path.Combine(RootPath, "Characters");

        public static string BackupsPath =>
            Path.Combine(RootPath, "Backups");

        public static string GetMetadataPath(string uid) =>
            Path.Combine(CharactersPath, $"{uid}.xml");

        public static string GetBackupsDir(string uid) =>
            Path.Combine(BackupsPath, uid);
    }
}
