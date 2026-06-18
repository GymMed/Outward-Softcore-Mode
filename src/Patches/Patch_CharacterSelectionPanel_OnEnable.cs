using HarmonyLib;
using OutwardSoftcoreMode.Services;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(CharacterSelectionPanel), "OnEnable")]
    public class Patch_CharacterSelectionPanel_OnEnable
    {
        static void Postfix()
        {
            SoftcoreSaveManager.RestoreOrphanedBackups();
        }
    }
}
