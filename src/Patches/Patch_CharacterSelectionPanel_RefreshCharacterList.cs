using HarmonyLib;
using OutwardSoftcoreMode.Services;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(CharacterSelectionPanel), nameof(CharacterSelectionPanel.RefreshCharacterList))]
    public class Patch_CharacterSelectionPanel_RefreshCharacterList
    {
        static void Postfix(CharacterSelectionPanel __instance)
        {
            if (__instance.PlayerID <= 0)
                return;

            var savesField = AccessTools.Field(typeof(CharacterSelectionPanel), "m_saveSlot");
            var saves = savesField.GetValue(__instance) as System.Collections.IList;
            if (saves == null)
                return;

            var charSaves = SaveManager.Instance?.CharacterSaves;
            if (charSaves == null)
                return;

            for (int i = 0; i < saves.Count && i < charSaves.Count; i++)
            {
                var slot = saves[i];
                if (slot == null)
                    continue;

                var slotType = slot.GetType();
                var hcProp = slotType.GetProperty("HarcoreMode");
                if (hcProp == null)
                    continue;

                bool isHardcore = (bool)hcProp.GetValue(slot, null);
                if (isHardcore == CharacterManager.Instance.HardcoreMode)
                    continue;

                string uid = charSaves[i]?.CharacterUID;
                if (!string.IsNullOrEmpty(uid) && SoftcoreSaveManager.IsSoftcoreCharacter(uid))
                {
                    var interactableProp = slotType.GetProperty("interactable");
                    if (interactableProp != null)
                        interactableProp.SetValue(slot, true, null);
                }
            }
        }
    }
}
