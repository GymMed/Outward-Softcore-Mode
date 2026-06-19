using HarmonyLib;
using OutwardSoftcoreMode.Events;
using OutwardSoftcoreMode.Services;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(SaveInstance), nameof(SaveInstance.Save))]
    public class Patch_SaveInstance_Save
    {
        static void Postfix(SaveInstance __instance, bool _saveChar, bool _saveWorld)
        {
            string uid = __instance.SaveID;
            if (string.IsNullOrEmpty(uid) || !OutwardSoftcoreMode.PendingManualBackupUIDs.Contains(uid))
                return;

            string timestamp = __instance.InstancePath;

            if (string.IsNullOrEmpty(timestamp))
            {
                OutwardSoftcoreMode.LogMessage("Cannot backup: missing InstancePath");
                return;
            }

            if (!SoftcoreSaveManager.IsSoftcoreCharacter(uid))
            {
                OutwardSoftcoreMode.LogMessage("Cannot backup: character is not softcore");
                return;
            }

            string name = GetLocalCharacterNameByUID(uid);
            SoftcoreSaveManager.EnsureMetadataExists(uid, name);
            SoftcoreSaveManager.CreateBackup(uid, timestamp);
            EventBusPublisher.PublishSaveBackupAfter(uid);

            OutwardSoftcoreMode.PendingManualBackupUIDs.Remove(uid);
            OutwardSoftcoreMode.LogMessage("Manual softcore backup completed");
        }

        private static string GetLocalCharacterNameByUID(string uid)
        {
            foreach (var player in SplitScreenManager.Instance.LocalPlayers)
            {
                if (player.AssignedCharacter?.UID == uid)
                    return player.AssignedCharacter.Name;
            }
            return "Unknown";
        }
    }
}
