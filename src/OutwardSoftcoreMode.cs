using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SideLoader;
using OutwardModsCommunicator;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using OutwardModsCommunicator.EventBus;
using OutwardSoftcoreMode.Events;
using OutwardSoftcoreMode.Services;
using OutwardSoftcoreMode.BepInEx.Configs;

namespace OutwardSoftcoreMode
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SideLoader.SL.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(OutwardModsCommunicator.OMC.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class OutwardSoftcoreMode : BaseUnityPlugin
    {
        public const string GUID = "gymmed.softcore_mode";
        public const string NAME = "Softcore Mode";
        public const string VERSION = "0.0.2";

        public static string prefix = "[Softcore-Mode]";

        public const string EVENTS_LISTENER_GUID = GUID + "_*";

        internal static ManualLogSource Log;

        public static ConfigEntry<int> MaxBackups => BackupsConfigs.MaxBackups;
        public static ConfigEntry<float> SaveCooldownHours => SaveCooldownConfigs.SaveCooldownHours;
        public static ConfigEntry<int> DeathChance => DeathChanceConfigs.DeathChance;

        public static bool IsCurrentGameSoftcore;
        internal static int PendingSoftcoreCount;
        internal static HashSet<string> PendingManualBackupUIDs = new HashSet<string>();
        internal static HashSet<string> PendingCooldownUIDs = new HashSet<string>();

        internal void Awake()
        {
            Log = this.Logger;
            LogMessage($"Hello world from {NAME} {VERSION}!");

            BackupsConfigs.Init(this);
            SaveCooldownConfigs.Init(this);
            DeathChanceConfigs.Init(this);

            new Harmony(GUID).PatchAll();

            EventBusRegister.RegisterEvents();
            //EventBusSubscriber.AddSubscribers();
        }

        internal void Update()
        {
            if (PendingCooldownUIDs.Count > 0)
            {
                float gameTime = EnvironmentConditions.GameTimeF;
                if (gameTime > 0f)
                {
                    foreach (string uid in PendingCooldownUIDs)
                    {
                        SoftcoreSaveManager.SetLastBackupGameTime(uid, gameTime);
                        SoftcoreSaveManager.ClearRestoredFlag(uid);
                        DebugLog($"Cooldown set to {gameTime:F2} for restored {uid} (deferred)");
                    }
                    PendingCooldownUIDs.Clear();
                }
            }
        }

        public static void LogMessage(string message)
        {
            Log.LogMessage($"{OutwardSoftcoreMode.prefix} {message}");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugLog(string message) => Log.LogMessage($"{prefix} [DEBUG] {message}");

        public static void LogSL(string message)
        {
            SL.Log($"{OutwardSoftcoreMode.prefix} {message}");
        }

        public static string GetProjectLocation()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
