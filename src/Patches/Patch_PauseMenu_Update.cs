using HarmonyLib;
using UnityEngine;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.Update))]
    public class Patch_PauseMenu_Update
    {
        private static float _nextRefresh;

        static void Postfix(PauseMenu __instance)
        {
            if (!__instance.IsDisplayed)
            {
                _nextRefresh = 0f;
                return;
            }

            if (Time.unscaledTime < _nextRefresh)
                return;

            _nextRefresh = Time.unscaledTime + 0.5f;

            Patch_PauseMenu_AddSaveButton.RefreshSoftcoreSaveButton(__instance);
        }
    }
}
