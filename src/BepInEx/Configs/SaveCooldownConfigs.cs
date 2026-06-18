using BepInEx;
using BepInEx.Configuration;

namespace OutwardSoftcoreMode.BepInEx.Configs
{
    public static class SaveCooldownConfigs
    {
        public static ConfigEntry<float> SaveCooldownHours;

        public static void Init(BaseUnityPlugin plugin)
        {
            SaveCooldownHours = plugin.Config.Bind(
                "Backups",
                "SaveCooldownHours",
                24f,
                "Minimum game hours between manual backups. Set to 0 to disable cooldown."
            );
        }
    }
}
