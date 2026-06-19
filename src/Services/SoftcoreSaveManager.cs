using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using HarmonyLib;
using UnityEngine;

namespace OutwardSoftcoreMode.Services
{
    public static class SoftcoreSaveManager
    {
        private static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(SoftcorePaths.CharactersPath);
            Directory.CreateDirectory(SoftcorePaths.BackupsPath);
        }

        private static XDocument LoadMetadataOrNull(string uid)
        {
            string path = SoftcorePaths.GetMetadataPath(uid);
            if (!File.Exists(path))
                return null;
            try { return XDocument.Load(path); }
            catch { return null; }
        }

        private static void ModifyMetadata(string uid, Action<XElement> transform)
        {
            string path = SoftcorePaths.GetMetadataPath(uid);
            if (!File.Exists(path))
                return;
            try
            {
                var doc = XDocument.Load(path);
                transform(doc.Root);
                doc.Save(path);
            }
            catch (Exception ex)
            {
                OutwardSoftcoreMode.LogMessage($"Failed to modify metadata: {ex.Message}");
            }
        }

        public static bool IsSoftcoreCharacter(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                return false;
            return File.Exists(SoftcorePaths.GetMetadataPath(uid))
                || Directory.Exists(SoftcorePaths.GetBackupsDir(uid));
        }

        public static string GetCharacterName(string uid)
        {
            return LoadMetadataOrNull(uid)?.Root?.Element("Name")?.Value;
        }

        public static void WriteMetadata(string uid, string name)
        {
            EnsureDirectoriesExist();
            var doc = new XDocument(
                new XElement("SoftcoreCharacter",
                    new XElement("Name", name ?? "Unknown"),
                    new XElement("PermanentDeathCount", 0),
                    new XElement("LastBackupGameTime", -1),
                    new XElement("CooldownDuration", -1f)
                )
            );
            doc.Save(SoftcorePaths.GetMetadataPath(uid));
        }

        public static int GetPermanentDeathCount(string uid)
        {
            var doc = LoadMetadataOrNull(uid);
            if (doc == null)
                return 0;
            var root = doc.Root;
            var element = root?.Element("PermanentDeathCount");
            if (element != null && int.TryParse(element.Value, out int count))
                return count;
            element = root?.Element("DeathCount");
            return element != null && int.TryParse(element.Value, out count) ? count : 0;
        }

        public static void IncrementPermanentDeathCount(string uid)
        {
            int oldCount = GetPermanentDeathCount(uid);
            OutwardSoftcoreMode.DebugLog($"IncrementPermanentDeathCount: uid={uid}, oldCount={oldCount}, path={SoftcorePaths.GetMetadataPath(uid)}");

            string path = SoftcorePaths.GetMetadataPath(uid);
            if (!File.Exists(path))
            {
                string name = GetCharacterName(uid) ?? "Unknown";
                WriteMetadata(uid, name);
            }

            ModifyMetadata(uid, root =>
            {
                var element = root.Element("PermanentDeathCount");
                if (element != null)
                {
                    int.TryParse(element.Value, out int count);
                    element.Value = (count + 1).ToString();
                }
                else
                {
                    root.Add(new XElement("PermanentDeathCount", 1));
                }
            });

            int newCount = GetPermanentDeathCount(uid);
            OutwardSoftcoreMode.DebugLog($"IncrementPermanentDeathCount: uid={uid}, newCount={newCount}");
        }

        public static void EnsureMetadataExists(string uid, string name)
        {
            string path = SoftcorePaths.GetMetadataPath(uid);
            if (File.Exists(path))
                return;

            WriteMetadata(uid, name ?? "Unknown");
            OutwardSoftcoreMode.DebugLog($"Created metadata for {uid} (name={name})");
        }

        public static float GetLastBackupGameTime(string uid)
        {
            float backupTime = BackupMetadataStore.GetLatestBackupGameTime(uid);
            if (backupTime >= 0f)
                return backupTime;

            var doc = LoadMetadataOrNull(uid);
            if (doc == null)
                return -1f;
            var element = doc.Root?.Element("LastBackupGameTime");
            return element != null && float.TryParse(element.Value, out float time) ? time : -1f;
        }

        public static void SetLastBackupGameTime(string uid, float gameTime)
        {
            ModifyMetadata(uid, root =>
            {
                var element = root.Element("LastBackupGameTime");
                if (element != null)
                    element.Value = gameTime.ToString("F2");
                else
                    root.Add(new XElement("LastBackupGameTime", gameTime.ToString("F2")));
            });
        }

        private static (bool canBackup, float remainingTime) GetCooldownState(string uid)
        {
            float configCooldown = OutwardSoftcoreMode.SaveCooldownHours?.Value ?? 24f;
            if (configCooldown <= 0f)
                return (true, 0f);

            float lastBackup = GetLastBackupGameTime(uid);
            if (lastBackup < 0f)
                return (true, 0f);

            float storedDuration = ReadStoredCooldownDuration(uid, configCooldown);
            float currentTime = EnvironmentConditions.GameTimeF;
            float elapsed = currentTime - lastBackup;
            float remaining = Math.Max(0f, storedDuration - elapsed);
            return (elapsed >= storedDuration, remaining);
        }

        public static bool CanBackupNow(string uid) => GetCooldownState(uid).canBackup;

        public static float GetRemainingCooldownTime(string uid) => GetCooldownState(uid).remainingTime;

        public static string GetCharacterSaveDirectory(string uid) =>
            Path.Combine(SaveManager.GetSavePath(), $"Save_{uid}");

        private static void CopyDirectoryContents(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string dest = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }
        }

        public static bool IsRestoredBackupInstance(string uid, string instancePath)
        {
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(instancePath))
                return false;

            return Directory.Exists(Path.Combine(SoftcorePaths.GetBackupsDir(uid), instancePath));
        }

        private static float ReadStoredCooldownDuration(string uid, float fallback)
        {
            var doc = LoadMetadataOrNull(uid);
            if (doc == null)
                return fallback;
            var element = doc.Root?.Element("CooldownDuration");
            if (element != null && float.TryParse(element.Value, out float duration) && duration > 0f)
                return duration;
            return fallback;
        }

        private static void WriteCooldownDuration(string uid, float duration)
        {
            ModifyMetadata(uid, root =>
            {
                var element = root.Element("CooldownDuration");
                if (element != null)
                    element.Value = duration.ToString("F2");
                else
                    root.Add(new XElement("CooldownDuration", duration.ToString("F2")));
            });
        }

        public static void CreateBackup(string uid, string instanceTimestamp)
        {
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(instanceTimestamp))
                return;

            EnsureDirectoriesExist();

            string sourceDir = Path.Combine(GetCharacterSaveDirectory(uid), instanceTimestamp);
            if (!Directory.Exists(sourceDir))
            {
                OutwardSoftcoreMode.LogMessage($"Backup source not found: {sourceDir}");
                return;
            }

            string backupDir = Path.Combine(SoftcorePaths.GetBackupsDir(uid), instanceTimestamp);
            CopyDirectoryContents(sourceDir, backupDir);

            OutwardSoftcoreMode.LogMessage($"Backup created for {uid} at {instanceTimestamp}");

            float gameTime = EnvironmentConditions.GameTimeF;
            float cooldownDuration;

            if (OutwardSoftcoreMode.CooldownRandomizerEnabled?.Value == true)
            {
                float min = OutwardSoftcoreMode.CooldownRandomizerMinHours?.Value ?? 24f;
                float max = OutwardSoftcoreMode.CooldownRandomizerMaxHours?.Value ?? 168f;
                cooldownDuration = UnityEngine.Random.Range(min, max);
            }
            else
            {
                cooldownDuration = OutwardSoftcoreMode.SaveCooldownHours?.Value ?? 24f;
            }

            BackupMetadataStore.WriteGameTime(uid, instanceTimestamp, gameTime);
            SetLastBackupGameTime(uid, gameTime);
            WriteCooldownDuration(uid, cooldownDuration);

            EnforceBackupLimit(uid);
        }

        private static void EnforceBackupLimit(string uid)
        {
            string backupsDir = SoftcorePaths.GetBackupsDir(uid);
            if (!Directory.Exists(backupsDir))
                return;

            int max = OutwardSoftcoreMode.MaxBackups?.Value ?? 10;
            var dirs = new DirectoryInfo(backupsDir)
                .GetDirectories()
                .OrderByDescending(d => d.Name)
                .ToList();

            while (dirs.Count > max)
            {
                var oldest = dirs[dirs.Count - 1];
                try
                {
                    oldest.Delete(true);
                    OutwardSoftcoreMode.LogMessage($"Deleted old backup: {oldest.Name}");
                }
                catch (Exception ex)
                {
                    OutwardSoftcoreMode.LogMessage($"Failed to delete old backup: {ex.Message}");
                }
                dirs.RemoveAt(dirs.Count - 1);
            }
        }

        private static HashSet<string> GetActiveCharacterUIDs()
        {
            var uids = new HashSet<string>();
            if (SaveManager.Instance?.CharacterSaves == null)
                return uids;

            foreach (var holder in SaveManager.Instance.CharacterSaves)
            {
                if (holder != null)
                    uids.Add(holder.CharacterUID);
            }

            return uids;
        }

        public static void RestoreOrphanedBackups()
        {
            string oldIndexPath = Path.Combine(SoftcorePaths.RootPath, "restored_instances.xml");
            if (File.Exists(oldIndexPath))
            {
                try
                {
                    File.Delete(oldIndexPath);
                    OutwardSoftcoreMode.LogMessage("Cleaned up legacy restored_instances.xml index");
                }
                catch (Exception ex)
                {
                    OutwardSoftcoreMode.LogMessage($"Failed to delete legacy index: {ex.Message}");
                }
            }

            if (!Directory.Exists(SoftcorePaths.BackupsPath))
            {
                OutwardSoftcoreMode.LogMessage("RestoreOrphanedBackups: no backups directory");
                return;
            }

            var activeUIDs = GetActiveCharacterUIDs();
            string[] backupDirs = Directory.GetDirectories(SoftcorePaths.BackupsPath);

            OutwardSoftcoreMode.LogMessage(
                $"RestoreOrphanedBackups: scanning {backupDirs.Length} backup dirs, {activeUIDs.Count} active characters");

            int restoredCount = 0;
            foreach (string dir in backupDirs)
            {
                string uid = Path.GetFileName(dir);
                if (string.IsNullOrEmpty(uid) || activeUIDs.Contains(uid))
                    continue;

                OutwardSoftcoreMode.LogMessage($"RestoreOrphanedBackups: restoring {uid}");
                RestoreCharacter(uid);
                restoredCount++;
            }

            OutwardSoftcoreMode.LogMessage($"RestoreOrphanedBackups: restored {restoredCount} characters");

            if (restoredCount > 0)
                RefreshCharacterSelectionPanels();
        }

        public static bool IsRestoredBackupCharacter(string uid)
        {
            var doc = LoadMetadataOrNull(uid);
            if (doc == null)
                return false;
            var element = doc.Root?.Element("IsRestored");
            return element != null && bool.TryParse(element.Value, out bool restored) && restored;
        }

        public static void SetRestoredFlag(string uid)
        {
            string path = SoftcorePaths.GetMetadataPath(uid);
            if (!File.Exists(path))
            {
                OutwardSoftcoreMode.DebugLog($"SetRestoredFlag: no metadata to flag for {uid} — skipping");
                return;
            }

            for (int attempt = 0; attempt < 2; attempt++)
            {
                ModifyMetadata(uid, root =>
                {
                    var el = root.Element("IsRestored");
                    if (el != null)
                        el.Value = "true";
                    else
                        root.Add(new XElement("IsRestored", "true"));
                });

                if (IsRestoredBackupCharacter(uid))
                    return;
            }

            OutwardSoftcoreMode.LogMessage($"Failed to persist IsRestored flag for {uid}");
        }

        public static void ClearRestoredFlag(string uid)
        {
            if (!File.Exists(SoftcorePaths.GetMetadataPath(uid)))
                return;

            ModifyMetadata(uid, root =>
            {
                var el = root.Element("IsRestored");
                if (el != null)
                    el.Value = "false";
            });
        }

        private static void RestoreCharacter(string uid)
        {
            string backupsDir = SoftcorePaths.GetBackupsDir(uid);
            if (!Directory.Exists(backupsDir))
                return;

            var backups = new DirectoryInfo(backupsDir)
                .GetDirectories()
                .OrderByDescending(d => d.Name)
                .ToList();

            if (backups.Count == 0)
                return;

            string saveDir = GetCharacterSaveDirectory(uid);
            foreach (var backup in backups)
            {
                string destDir = Path.Combine(saveDir, backup.Name);
                if (Directory.Exists(destDir))
                    continue;

                CopyDirectoryContents(backup.FullName, destDir);
                OutwardSoftcoreMode.LogMessage($"Restored backup {backup.Name} for {uid}");
            }

            SetRestoredFlag(uid);
            RegisterRestoredCharacter(uid);
        }

        private static void RegisterRestoredCharacter(string uid)
        {
            string saveDir = GetCharacterSaveDirectory(uid);
            if (!Directory.Exists(saveDir))
                return;

            var holder = CharacterSaveInstanceHolder.PrepareCharacterSaveInstanceHolder(uid, saveDir);
            if (holder == null)
            {
                OutwardSoftcoreMode.LogMessage($"RegisterRestoredCharacter: PrepareCharacterSaveInstanceHolder returned null for {uid}");
                return;
            }

            var charSaves = AccessTools
                .Field(typeof(SaveManager), "m_charSaves")
                .GetValue(SaveManager.Instance);
            if (charSaves == null)
                return;

            var containsKey = AccessTools.Method(charSaves.GetType(), "ContainsKey");
            var remove = AccessTools.Method(charSaves.GetType(), "Remove");
            var add = AccessTools.Method(charSaves.GetType(), "Add");

            bool exists = (bool)containsKey.Invoke(charSaves, new object[] { uid });
            if (exists)
                remove.Invoke(charSaves, new object[] { uid });

            add.Invoke(charSaves, new object[] { uid, holder });
        }

        private static void RefreshCharacterSelectionPanels()
        {
            var panels = Resources.FindObjectsOfTypeAll<CharacterSelectionPanel>();
            foreach (var panel in panels)
            {
                if (panel.isActiveAndEnabled)
                {
                    var refresh = AccessTools.Method(typeof(CharacterSelectionPanel), "RefreshCharacterList");
                    refresh.Invoke(panel, null);
                }
            }
        }
    }
}
