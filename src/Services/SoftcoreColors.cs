using UnityEngine;
using UnityEngine.UI;

namespace OutwardSoftcoreMode.Services
{
    public static class SoftcoreColors
    {
        public const string PurpleHex = "#A855F7";

        public static Color Purple => new Color(0.6588f, 0.3333f, 0.9686f);

        public static void DestroyLocalize(GameObject obj)
        {
            foreach (var uil in obj.GetComponentsInChildren<UILocalize>(true))
                Object.Destroy(uil);
        }

        public static GameObject CreateSoftcoreLabel(GameObject template, Transform parent, string name)
        {
            GameObject label = Object.Instantiate(template, parent);
            label.name = name;
            DestroyLocalize(label);
            return label;
        }
    }
}
