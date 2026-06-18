using HarmonyLib;
using OutwardSoftcoreMode.Services;
using UnityEngine;
using UnityEngine.UI;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(CharacterSaveSlot), nameof(CharacterSaveSlot.SetSave))]
    public class Patch_CharacterSaveSlot_SetSave
    {
        static void Postfix(CharacterSaveSlot __instance, CharacterSaveInstanceHolder _saveContainer)
        {
            string uid = _saveContainer.CharacterUID;

            Transform hardcoreFlag = (Transform)AccessTools.Field(typeof(CharacterSaveSlot), "m_hardcoreFlag").GetValue(__instance);
            if (hardcoreFlag == null)
                return;

            Transform existing = hardcoreFlag.parent.Find("lblSoftcore");

            if (!SoftcoreSaveManager.IsSoftcoreCharacter(uid))
            {
                if (existing != null)
                    existing.gameObject.SetActive(false);
                return;
            }

            GameObject label = existing?.gameObject;
            if (label == null)
                label = SoftcoreColors.CreateSoftcoreLabel(hardcoreFlag.gameObject, hardcoreFlag.parent, "lblSoftcore");

            label.SetActive(true);

            Text text = label.GetComponentInChildren<Text>();
            if (text != null)
            {
                int deaths = SoftcoreSaveManager.GetPermanentDeathCount(uid);
                text.text = deaths > 0 ? $"Softcore {deaths}" : "Softcore";
                text.color = SoftcoreColors.Purple;
            }

            hardcoreFlag.gameObject.SetActive(false);
        }
    }
}
