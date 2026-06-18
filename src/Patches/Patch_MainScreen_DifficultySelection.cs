using HarmonyLib;
using OutwardSoftcoreMode.Services;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(MainScreen), nameof(MainScreen.OnSplitModeChosen))]
    public class Patch_MainScreen_DifficultySelection
    {
        private const string SoftcoreDisclaimer =
            "Are you sure you want to play in softcore mode? \n<color=" + SoftcoreColors.PurpleHex + ">In softcore mode, when you are defeated there is a chance that your character dies PERMANENTLY, but you can create manual save backups to restore if needed.</color>\n\nAdditional note: When playing coop, players must be in the same mode to play together.";

        static bool Prefix(MainScreen __instance, bool _splitActive)
        {
            OutwardSoftcoreMode.PendingSoftcoreCount = 0;
            OutwardSoftcoreMode.IsCurrentGameSoftcore = false;

            string loc = LocalizationManager.Instance.GetLoc("CharacterCreation_Mode_GameMode");
            string[] buttons = new string[] { "General_DifficultyNormal", "Softcore", "General_DifficultyHardcore" };
            var onHardcore = AccessTools.Method(typeof(MainScreen), "OnHardcoreModeChosen");

            if (!_splitActive)
            {
                __instance.m_characterUI.MessagePanel.Show(
                    loc, null, buttons,
                    delegate { __instance.ShowCharacterCreation(false); },
                    delegate { OnSoftcoreModeChosen(__instance, false); },
                    delegate { onHardcore.Invoke(__instance, new object[] { false }); }
                );
            }
            else
            {
                __instance.m_characterUI.MessagePanel.Show(
                    loc, null, buttons,
                    delegate { __instance.ShowCharacterCreation2P(false); },
                    delegate { OnSoftcoreModeChosen(__instance, true); },
                    delegate { onHardcore.Invoke(__instance, new object[] { true }); }
                );
            }

            return false;
        }

        private static void OnSoftcoreModeChosen(MainScreen instance, bool splitActive)
        {
            if (Global.AudioManager)
                Global.AudioManager.PlaySound(GlobalAudioManager.Sounds.UI_GENERAL_Click, 0f, 1f, 1f, 1f, 1f);

            instance.m_characterUI.MessagePanel.Show(
                SoftcoreDisclaimer, null,
                MakeConfirmAction(splitActive, instance),
                delegate { instance.m_optionButtonsPanel.Focus(); },
                true, -1f, null
            );
        }

        private static UnityAction MakeConfirmAction(bool splitActive, MainScreen instance)
        {
            int count = splitActive ? 2 : 1;
            if (splitActive)
                return () => { OutwardSoftcoreMode.PendingSoftcoreCount = count; instance.ShowCharacterCreation2P(true); };
            else
                return () => { OutwardSoftcoreMode.PendingSoftcoreCount = count; instance.ShowCharacterCreation(true); };
        }
    }
}
