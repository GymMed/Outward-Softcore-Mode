using System.Collections.Generic;
using HarmonyLib;
using OutwardSoftcoreMode.Services;
using UnityEngine;

namespace OutwardSoftcoreMode.Patches
{
    [HarmonyPatch(typeof(DefeatScenariosManager), nameof(DefeatScenariosManager.ActivateDefeatScenario))]
    public class Patch_DefeatScenariosManager_ActivateDefeatScenario
    {
        private static readonly List<string> _defeatedCharacterUIDs = new List<string>();
        private static bool _deathTriggered;
        private static bool _suppressedHardcore;
        private static bool _originalSupportHardcore;

        static bool Prefix(DefeatScenario _scenario)
        {
            _defeatedCharacterUIDs.Clear();
            _deathTriggered = false;
            _suppressedHardcore = false;
            _originalSupportHardcore = false;

            var cm = CharacterManager.Instance;
            AddIfDead(cm?.GetFirstLocalCharacter());
            AddIfDead(cm?.GetSecondLocalCharacter());

            if (_defeatedCharacterUIDs.Count == 0)
                return true;

			bool anySoftcoreDead = false;
			var processed = new HashSet<string>();
			var softcoreUIDs = new List<string>();

			foreach (string uid in _defeatedCharacterUIDs)
			{
				if (string.IsNullOrEmpty(uid) || !processed.Add(uid))
					continue;

				if (!SoftcoreSaveManager.IsSoftcoreCharacter(uid))
					continue;

				anySoftcoreDead = true;
				softcoreUIDs.Add(uid);
			}

			if (anySoftcoreDead)
			{
				int chance = OutwardSoftcoreMode.DeathChance?.Value ?? 20;
				int roll = Random.Range(0, 100);

				if (roll < chance)
				{
					foreach (string uid in softcoreUIDs)
					{
						int oldCount = SoftcoreSaveManager.GetPermanentDeathCount(uid);
						SoftcoreSaveManager.IncrementPermanentDeathCount(uid);
						int newCount = SoftcoreSaveManager.GetPermanentDeathCount(uid);
						OutwardSoftcoreMode.LogMessage($"Softcore death for {uid} — permanent death count: {oldCount} -> {newCount}");
					}
					_deathTriggered = true;
				}
			}

            if (_deathTriggered)
            {
                var method = AccessTools.Method(typeof(DefeatScenariosManager), "DefeatHardcoreDeath");
                if (method != null)
                {
                    method.Invoke(DefeatScenariosManager.Instance, null);
                }
                else
                {
                    OutwardSoftcoreMode.LogMessage("DefeatHardcoreDeath method not found — setting HardcoreDeathTriggered directly");
                    CharacterManager.Instance.HardcoreDeathTriggered = true;
                }
                return false;
            }

            if (anySoftcoreDead && _scenario != null)
            {
                _suppressedHardcore = true;
                _originalSupportHardcore = _scenario.SupportHardcore;
                _scenario.SupportHardcore = false;
            }

            return true;
        }

        private static void AddIfDead(Character c)
        {
            if (c != null && c.Stats != null && c.Stats.CurrentHealth <= 0f)
                _defeatedCharacterUIDs.Add(c.UID);
        }

        static void Postfix(DefeatScenario _scenario)
        {
            if (_suppressedHardcore && _scenario != null)
                _scenario.SupportHardcore = _originalSupportHardcore;

            _defeatedCharacterUIDs.Clear();
            _deathTriggered = false;
            _suppressedHardcore = false;
            _originalSupportHardcore = false;
        }
    }
}
