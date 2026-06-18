using HarmonyLib;
using OutwardSoftcoreMode.Services;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.ConfirmAddNewSave))]
    public class Patch_SaveManager_ConfirmAddNewSave
    {
        static void Postfix(CharacterSave _newSave)
        {
            if (OutwardSoftcoreMode.PendingSoftcoreCount <= 0 && !OutwardSoftcoreMode.IsCurrentGameSoftcore)
                return;

            string uid = _newSave?.PSave?.UID;
            if (string.IsNullOrEmpty(uid))
            {
                OutwardSoftcoreMode.LogMessage("Cannot write softcore metadata: no UID in new save");
                OutwardSoftcoreMode.PendingSoftcoreCount = 0;
                return;
            }

            SoftcoreSaveManager.WriteMetadata(uid, _newSave.PSave.Name);
            OutwardSoftcoreMode.LogMessage($"Softcore metadata written for {_newSave.PSave.Name} ({uid})");

            OutwardSoftcoreMode.IsCurrentGameSoftcore = true;
            OutwardSoftcoreMode.PendingManualBackupUIDs.Add(uid);
            if (OutwardSoftcoreMode.PendingSoftcoreCount > 0)
                OutwardSoftcoreMode.PendingSoftcoreCount--;
        }
    }
}
