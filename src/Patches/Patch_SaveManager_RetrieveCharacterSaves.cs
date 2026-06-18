using HarmonyLib;
using OutwardSoftcoreMode.Services;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.RetrieveCharacterSaves))]
    public class Patch_SaveManager_RetrieveCharacterSaves
    {
        static void Postfix()
        {
            SoftcoreSaveManager.RestoreOrphanedBackups();
        }
    }
}
