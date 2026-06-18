using System.Collections.Generic;
using HarmonyLib;
using OutwardSoftcoreMode.Services;
using UnityEngine;
using UnityEngine.UI;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(CharacterSaveInstanceDisplay), nameof(CharacterSaveInstanceDisplay.SetSaveInstance))]
    public class Patch_CharacterSaveInstanceDisplay_SetSaveInstance
    {
        private const string LabelName = "lblSaveSoftcore";
        private const float RightLabelWidth = 339.1f;
        private const float LabelHeight = 35f;

        private struct AreaNameLayout
        {
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 pivot;
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
        }

        private static readonly Dictionary<int, AreaNameLayout> _savedAreaNameLayout = new Dictionary<int, AreaNameLayout>();

        static void Postfix(CharacterSaveInstanceDisplay __instance, SaveInstance _instance)
        {
            string uid = _instance?.SaveID;
            string path = _instance?.InstancePath;
            Transform data = __instance.transform.Find("Data");
            if (data == null)
                return;

            if (SoftcoreSaveManager.IsRestoredBackupInstance(uid, path))
            {
                SetupLayout(data);
                UpdateLabel(data, "Backup");
            }
            else
            {
                ResetLayout(data);
                HideLabel(data);
            }
        }

        private static RectTransform GetAreaNameRectTransform(Transform data)
        {
            Transform t = data.Find("lblAreaName");
            return t != null ? t.GetComponent<RectTransform>() : null;
        }

        private static void SaveOriginalAreaNameLayout(Transform data)
        {
            int key = data.GetInstanceID();
            if (_savedAreaNameLayout.ContainsKey(key))
                return;

            var rt = GetAreaNameRectTransform(data);
            if (rt == null)
                return;

            _savedAreaNameLayout[key] = new AreaNameLayout
            {
                anchorMin = rt.anchorMin,
                anchorMax = rt.anchorMax,
                pivot = rt.pivot,
                anchoredPosition = rt.anchoredPosition,
                sizeDelta = rt.sizeDelta
            };
        }

        private static void SetupLayout(Transform data)
        {
            var rt = GetAreaNameRectTransform(data);
            if (rt != null)
            {
                SaveOriginalAreaNameLayout(data);
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(RightLabelWidth, LabelHeight);
            }

            Transform existing = data.Find(LabelName);
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                return;
            }

            Transform lblTime = data.Find("lblTime");
            if (lblTime == null)
                return;

            GameObject label = SoftcoreColors.CreateSoftcoreLabel(lblTime.gameObject, data, LabelName);
            var labelRt = label.GetComponent<RectTransform>();
            if (labelRt != null)
            {
                labelRt.anchorMin = new Vector2(1, 0);
                labelRt.anchorMax = new Vector2(1, 0);
                labelRt.pivot = new Vector2(1, 0);
                labelRt.anchoredPosition = Vector2.zero;
                labelRt.sizeDelta = new Vector2(RightLabelWidth, LabelHeight);
            }
        }

        private static void UpdateLabel(Transform data, string text)
        {
            Transform t = data.Find(LabelName);
            if (t == null)
                return;

            Text txt = t.GetComponentInChildren<Text>();
            if (txt == null)
                return;

            txt.text = text;
            txt.color = SoftcoreColors.Purple;
            txt.alignment = TextAnchor.MiddleRight;
        }

        private static void ResetLayout(Transform data)
        {
            int key = data.GetInstanceID();
            var rt = GetAreaNameRectTransform(data);
            if (rt == null || !_savedAreaNameLayout.TryGetValue(key, out var saved))
                return;

            rt.anchorMin = saved.anchorMin;
            rt.anchorMax = saved.anchorMax;
            rt.pivot = saved.pivot;
            rt.anchoredPosition = saved.anchoredPosition;
            rt.sizeDelta = saved.sizeDelta;
            _savedAreaNameLayout.Remove(key);
        }

        private static void HideLabel(Transform data)
        {
            Transform t = data.Find(LabelName);
            if (t != null)
                t.gameObject.SetActive(false);
        }
    }
}
