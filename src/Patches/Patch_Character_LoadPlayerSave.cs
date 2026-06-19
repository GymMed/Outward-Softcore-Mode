using HarmonyLib;
using OutwardSoftcoreMode.Services;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(Character), nameof(Character.LoadPlayerSave))]
    public class Patch_Character_LoadPlayerSave
    {
        static void Postfix(Character __instance, PlayerSaveData _save)
        {
            string uid = __instance.UID;
            OutwardSoftcoreMode.IsCurrentGameSoftcore = SoftcoreSaveManager.IsSoftcoreCharacter(uid);

            if (!OutwardSoftcoreMode.IsCurrentGameSoftcore)
                return;

            if (SoftcoreSaveManager.IsRestoredBackupCharacter(uid))
            {
                OutwardSoftcoreMode.PendingCooldownUIDs.Add(uid);
                SoftcoreSaveManager.ClearRestoredFlag(uid);
                OutwardSoftcoreMode.DebugLog($"Cooldown deferred for restored character {uid}");
            }
            else if (SoftcoreSaveManager.GetLastBackupGameTime(uid) < 0f)
            {
                float gameTime = EnvironmentConditions.GameTimeF;
                SoftcoreSaveManager.SetLastBackupGameTime(uid, gameTime);
                OutwardSoftcoreMode.DebugLog($"Cooldown initialized to {gameTime:F2} for {uid}");
            }
        }
    }
}
