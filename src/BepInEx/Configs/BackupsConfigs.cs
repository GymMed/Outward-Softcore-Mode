using BepInEx;
using BepInEx.Configuration;

namespace OutwardSoftcoreMode.BepInEx.Configs
{
    public static class BackupsConfigs
    {
        public static ConfigEntry<int> MaxBackups;

        public static void Init(BaseUnityPlugin plugin)
        {
            MaxBackups = plugin.Config.Bind(
                "Backups",
                "MaxBackups",
                10,
                "Maximum number of backup instances kept per character. Oldest deleted when exceeded."
            );
        }
    }
}
