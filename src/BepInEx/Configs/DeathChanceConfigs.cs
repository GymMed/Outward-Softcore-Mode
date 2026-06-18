using BepInEx;
using BepInEx.Configuration;

namespace OutwardSoftcoreMode.BepInEx.Configs
{
    public static class DeathChanceConfigs
    {
        public static ConfigEntry<int> DeathChance;

        public static void Init(BaseUnityPlugin plugin)
        {
            DeathChance = plugin.Config.Bind(
                "Defeat",
                "DeathChance",
                20,
                new ConfigDescription(
                    "Permanent death chance on defeat (percentage). Minimum 20 (like hardcore), maximum 100.",
                    new AcceptableValueRange<int>(20, 100)
                )
            );
        }
    }
}
