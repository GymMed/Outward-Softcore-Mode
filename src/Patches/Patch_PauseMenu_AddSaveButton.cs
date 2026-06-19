using HarmonyLib;
using OutwardSoftcoreMode.Events;
using OutwardSoftcoreMode.Services;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.Show))]
    public class Patch_PauseMenu_AddSaveButton
    {
        static void Postfix(PauseMenu __instance)
        {
            string uid = __instance.LocalCharacter?.UID;
            bool isSoftcore = !string.IsNullOrEmpty(uid) && SoftcoreSaveManager.IsSoftcoreCharacter(uid);

            Transform container = GetButtonsContainer(__instance);
            if (container == null) return;

            Transform btnTransform = FindSoftcoreButton(container);

            if (isSoftcore)
            {
                if (btnTransform == null)
                {
                    CreateButton(container);
                    btnTransform = FindSoftcoreButton(container);
                }

                if (btnTransform != null)
                {
                    btnTransform.gameObject.SetActive(true);
                    RefreshSoftcoreSaveButton(__instance);
                }
            }
            else
            {
                if (btnTransform != null)
                    btnTransform.gameObject.SetActive(false);
            }
        }

        private static Transform GetButtonsContainer(PauseMenu menu)
        {
            var field = AccessTools.Field(typeof(PauseMenu), "m_hideOnPauseButtons");
            var obj = field?.GetValue(menu) as GameObject;
            return obj?.transform;
        }

        private static Transform FindSoftcoreButton(Transform container)
        {
            return container?.Find("btnSaveSoftcore");
        }

        private static Transform FindSoftcoreButton(PauseMenu menu)
        {
            var container = GetButtonsContainer(menu);
            return container != null ? FindSoftcoreButton(container) : null;
        }

        internal static void RefreshSoftcoreSaveButton(PauseMenu menu)
        {
            string uid = menu.LocalCharacter?.UID;
            if (string.IsNullOrEmpty(uid) || !SoftcoreSaveManager.IsSoftcoreCharacter(uid))
                return;

            Transform btnTransform = FindSoftcoreButton(menu);
            if (btnTransform == null) return;

            UpdateButtonText(btnTransform.gameObject, uid);
        }

        private static GameObject CreateButton(Transform parent)
        {
            GameObject template = parent.Find("btnSave")?.gameObject;
            if (template == null)
            {
                OutwardSoftcoreMode.LogMessage("Cannot create softcore save button: btnSave template not found");
                return null;
            }

            GameObject btn = Object.Instantiate(template, parent);
            btn.name = "btnSaveSoftcore";
            btn.SetActive(true);

            SoftcoreColors.DestroyLocalize(btn);

            Text text = btn.GetComponentInChildren<Text>();
            if (text != null)
                text.color = SoftcoreColors.Purple;

            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener((UnityAction)(OnSaveClicked));
            }

            return btn;
        }

        private static void OnSaveClicked()
        {
            OutwardSoftcoreMode.PendingManualBackupUIDs.Clear();

            foreach (var player in SplitScreenManager.Instance.LocalPlayers)
            {
                if (player.AssignedCharacter != null)
                {
                    string id = player.AssignedCharacter.UID;
                    if (SoftcoreSaveManager.IsSoftcoreCharacter(id) && SoftcoreSaveManager.CanBackupNow(id))
                        OutwardSoftcoreMode.PendingManualBackupUIDs.Add(id);
                }
            }

            if (OutwardSoftcoreMode.PendingManualBackupUIDs.Count > 0)
            {
                foreach (string uid in OutwardSoftcoreMode.PendingManualBackupUIDs)
                    EventBusPublisher.PublishSaveBackupBefore(uid);

                foreach (var menu in Resources.FindObjectsOfTypeAll<PauseMenu>())
                {
                    if (!menu.isActiveAndEnabled) continue;
                    Transform btn = FindSoftcoreButton(menu);
                    if (btn == null) continue;
                    Button b = btn.GetComponent<Button>();
                    if (b != null) b.interactable = false;
                }

                SaveManager.Instance.Save();
            }
        }

        private static void UpdateButtonText(GameObject btnObj, string uid)
        {
            Text text = btnObj.GetComponentInChildren<Text>();
            if (text == null)
                return;

            Button button = btnObj.GetComponent<Button>();
            bool canBackup = SoftcoreSaveManager.CanBackupNow(uid);

            if (canBackup)
            {
                text.text = "Save Backup";
                text.color = SoftcoreColors.Purple;
            }
            else
            {
                float remaining = SoftcoreSaveManager.GetRemainingCooldownTime(uid);
                text.text = $"Save {FormatCooldown(remaining)}";
                text.color = SoftcoreColors.Purple;
            }

            if (button != null)
                button.interactable = canBackup;
        }

        private static string FormatCooldown(float gameHours)
        {
            int totalMinutes = Mathf.CeilToInt(gameHours * 60f);
            int days = totalMinutes / (24 * 60);
            int hours = (totalMinutes % (24 * 60)) / 60;
            int minutes = totalMinutes % 60;

            return days > 0
                ? $"{days}:{hours:D2}:{minutes:D2}"
                : $"{hours}:{minutes:D2}";
        }
    }
}
