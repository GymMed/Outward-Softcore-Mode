using BepInEx;
using BepInEx.Configuration;

namespace OutwardSoftcoreMode.BepInEx.Configs
{
    public static class CooldownRandomizerConfigs
    {
        public static ConfigEntry<bool> CooldownRandomizerEnabled;
        public static ConfigEntry<float> CooldownRandomizerMinHours;
        public static ConfigEntry<float> CooldownRandomizerMaxHours;

        public static void Init(BaseUnityPlugin plugin)
        {
            CooldownRandomizerEnabled = plugin.Config.Bind(
                "Backup save cooldown randomizer",
                "CooldownRandomizerEnabled",
                true,
                "Enable random cooldown range for backup saves."
            );

            CooldownRandomizerMinHours = plugin.Config.Bind(
                "Backup save cooldown randomizer",
                "CooldownRandomizerMinHours",
                24f,
                "Minimum random cooldown in game hours."
            );

            CooldownRandomizerMaxHours = plugin.Config.Bind(
                "Backup save cooldown randomizer",
                "CooldownRandomizerMaxHours",
                168f,
                "Maximum random cooldown in game hours."
            );
        }
    }
}
