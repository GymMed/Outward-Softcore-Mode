<h1 align="center">
    Outward Softcore Mode
</h1>
<br/>

<div align="center">
  <img src="https://raw.githubusercontent.com/GymMed/Outward-Softcore-Mode/refs/heads/main/preview/images/1Logo.png" alt="Logo"/>
</div>

<div align="center">
	<a href="https://thunderstore.io/c/outward/p/GymMed/Softcore_Mode/">
		<img src="https://img.shields.io/thunderstore/dt/GymMed/Softcore_Mode" alt="Thunderstore Downloads">
	</a>
	<a href="https://github.com/GymMed/Outward-Softcore-Mode/releases/latest">
		<img src="https://img.shields.io/thunderstore/v/GymMed/Softcore_Mode" alt="Thunderstore Version">
	</a>
    <a href="https://github.com/GymMed/Outward-Mods-Communicator/releases/latest">
        <img src="https://img.shields.io/badge/Mods_Communicator-v1.2.0-9966ff" alt="Mods Communicator Version">
    </a>
</div>

<div align="center">
    Softcore — Hardcore with backups. A new game mode that lets you experience the thrill of being punished on death, without needing to start everything from zero.
</div>

> **Softcore Mode** was made for sessions where you're sprawled on the couch with a friend, drinks in hand, arguing about who forgot to check the map while one of you is getting mauled by a Shell Horror. You still want that jolt of panic on defeat — the "oh no, is this it?" moment — but you also have maybe three hours a week to play, and the thought of redoing all that progress is what kills the fun. This mode keeps the tension and the blame-game ("you were supposed to save the backup!") without demanding you start from scratch. Play risky, laugh about it, load your save.

## Dependencies

- [BepInEx-BepInExPack_Outward](https://outward.thunderstore.io/package/BepInEx/BepInExPack_Outward) (5.4.19)
- [sinai-dev-SideLoader](https://outward.thunderstore.io/package/sinai/dev-SideLoader) (3.8.4)
- [GymMed-Mods_Communicator](https://outward.thunderstore.io/package/GymMed/Mods_Communicator) (1.2.0)

## Features

<details>
<summary><b>Permanent Death Roll</b> — when a softcore character is defeated, a configurable percentage (default 20%) decides if they die permanently or respawn normally. Replaces the vanilla hardcore 20% roll with your own configurable rate.</summary>

Each defeat triggers a random roll against the `DeathChance` config value. If the roll hits, the character dies permanently — all local saves are deleted and the vanilla hardcore death screens play (loading screen, death context, return to main menu). If the roll misses, the character respawns through the normal defeat scenario.

The permanent death counter increments with each death. In split-screen, a single roll determines death for all softcore characters.

<details>
<summary>Game mode selection dialog — Normal, Softcore, and Hardcore options</summary>

<img src="https://raw.githubusercontent.com/GymMed/Outward-Softcore-Mode/refs/heads/main/preview/images/MainMenu-SoftcoreSelection.png" alt="Game mode selection with Normal, Softcore, and Hardcore options"/>
</details>

<details>
<summary>Softcore mode disclaimer warning before character creation</summary>

<img src="https://raw.githubusercontent.com/GymMed/Outward-Softcore-Mode/refs/heads/main/preview/images/MainMenu-SoftcoreSelection_Disclaimer.png" alt="Softcore mode disclaimer warning"/>
</details>

<details>
<summary><b>Technical Details</b></summary>

- **Patch target:** Harmony Prefix on `DefeatScenariosManager.ActivateDefeatScenario`
- **Softcore detection:** Per-character `IsSoftcoreCharacter(uid)` check (reads metadata XML). This correctly handles split-screen where both players are softcore.
- **Death path:** A single configurable roll decides all softcore characters' fate. If the roll hits, every softcore character at 0 HP has its permanent death count incremented. The Prefix then calls the vanilla `DefeatHardcoreDeath()` method via `AccessTools.Method` reflection, reusing the entire vanilla death UI sequence (loading screen → hardcore death context screens → return to main menu). The Prefix returns `false` to skip the original method. The vanilla 20% hardcore roll inside the original method never executes.
- **Survival path:** When the roll misses but softcore characters are at 0 HP, the Prefix sets `_scenario.SupportHardcore = false` before letting the original method run. This causes the vanilla 20% hardcore check on line 204 of `DefeatScenariosManager` to short-circuit, guaranteeing survival. The original value is saved and restored in the Postfix.
- **Death count storage:** `BepInEx/config/gymmed.Mods_Communicator/Softcore_Mode/Characters/{UID}.xml` — stores `PermanentDeathCount` element incremented on each death.
- **Config:** `DeathChance` (int, 20-100, default 20) in `gymmed.softcore_mode.cfg`.

</details>
</details>

<details>
<summary><b>Save Backup Button</b> — a purple "Save Backup" button in the pause menu creates full snapshots of your save file on demand, letting you manually preserve progress before risky encounters.</summary>

The button appears alongside the normal Save button for softcore characters. Clicking it triggers a save followed by a full copy of the save instance directory to a protected backup folder. A configurable cooldown (default 24 in-game hours) prevents spam — the button displays the remaining time when inactive. Oldest backups are automatically pruned when the configurable limit (default 10) is exceeded.

<details>
<summary>Save Backup button in the pause menu, colored purple</summary>

<img src="https://raw.githubusercontent.com/GymMed/Outward-Softcore-Mode/refs/heads/main/preview/images/PauseSettings-Save_Backup.png" alt="Save Backup button in the pause menu"/>
</details>

<details>
<summary>Save Backup button showing the cooldown timer</summary>

<img src="https://raw.githubusercontent.com/GymMed/Outward-Softcore-Mode/refs/heads/main/preview/images/PauseSettings-Save_Backup_Timer.png" alt="Save Backup button showing cooldown timer"/>
</details>

<details>
<summary><b>Technical Details</b></summary>

- **Button injection:** Harmony Postfix on `PauseMenu.Show` — clones the `btnSave` GameObject from `m_hideOnPauseButtons`, renames it `btnSaveSoftcore`, destroys `UILocalize` components, sets text color to purple (`#A855F7`), and registers a click listener.
- **Backup trigger flow:** Click listener → clears `PendingManualBackupUIDs` → adds each eligible local softcore player's UID → disables button → calls `SaveManager.Instance.Save()`. Harmony Postfix on `SaveInstance.Save` intercepts the completed save for any UID in the pending set and calls `SoftcoreSaveManager.CreateBackup()`.
- **File copy:** Copies all `.defedc`, `.deenvc`, and `Manifest.txt` files from `Save_{uid}/{instanceTimestamp}/` to `BepInEx/config/gymmed.Mods_Communicator/Softcore_Mode/Backups/{uid}/{instanceTimestamp}/`.
- **Cooldown display:** Harmony Postfix on `PauseMenu.Update` calls `RefreshSoftcoreSaveButton()` every 0.5 seconds, which reads `GetRemainingCooldownTime(uid)` and formats it as `D:HH:MM` or `HH:MM`.
- **Config:** `SaveCooldownHours` (float, default 24.0), `MaxBackups` (int, default 10) in `gymmed.softcore_mode.cfg`.

</details>
</details>

<details>
<summary><b>Backup Restoration</b> — if permanent death deletes your character, the mod automatically restores the newest backup when you return to the main menu. Your character reappears in the selection list.</summary>

When you reach the character select screen, the mod scans all backup directories. Any backed-up character whose UID is missing from the active save list is restored. The restore copies all save instance directories from the backup back into the game's save folder, then registers the character in the save system so it appears in the selection list. Restored save instances display a purple "Backup" label to distinguish them from original saves.

<details>
<summary>Restored save instances labeled "Backup" in the save selection list</summary>

<img src="https://raw.githubusercontent.com/GymMed/Outward-Softcore-Mode/refs/heads/main/preview/images/SaveSelectionWithBackups.png" alt="Restored save instances labeled Backup"/>
</details>

<details>
<summary><b>Technical Details</b></summary>

- **Trigger points:** `SoftcoreSaveManager.RestoreOrphanedBackups()` is called from two Harmony Postfix patches: `CharacterSelectionPanel.OnEnable` (whenever the character select panel is shown) and `SaveManager.RetrieveCharacterSaves` (when saves are refreshed).
- **Detection:** Compares each backup directory name (which is the character UID) against `SaveManager.Instance.CharacterSaves`. Restores any UID present in backups but absent from active saves.
- **Restore flow:** Sorts backup instance directories by name descending (newest first) → for each backup, copies its contents to `Save_{uid}/{backup.Name}/` only if the destination doesn't already exist → sets `IsRestored=true` in metadata XML → registers the character via reflection by calling `CharacterSaveInstanceHolder.PrepareCharacterSaveInstanceHolder(uid, saveDir)` and adding the result to `SaveManager`'s private `m_charSaves` dictionary → refreshes all open character selection panels.

</details>
</details>

<details>
<summary><b>Visual Labels</b> — softcore characters show a purple "Softcore" label with death count on the character select screen; restored save instances show a "Backup" label in the save list.</summary>

On the character select screen, softcore characters display a purple "Softcore" label (or "Softcore N" where N is the permanent death count) in place of the hardcore skull icon. In the save instance list (shown when selecting which save to load), any slot that was restored from a backup gets a purple "Backup" badge. These visual cues help you identify softcore characters and distinguish restored saves from originals at a glance.

<details>
<summary>Softcore label with permanent death count on the character select screen</summary>

<img src="https://raw.githubusercontent.com/GymMed/Outward-Softcore-Mode/refs/heads/main/preview/images/CharacterSelectionWithPermanentDeathCounts.png" alt="Softcore label with permanent death count"/>
</details>

<details>
<summary>Backup label on restored save instances</summary>

<img src="https://raw.githubusercontent.com/GymMed/Outward-Softcore-Mode/refs/heads/main/preview/images/SaveSelectionWithBackups.png" alt="Save instances labeled Backup after restoration"/>
</details>

<details>
<summary><b>Technical Details</b></summary>

- **Character select label:** Harmony Postfix on `CharacterSaveSlot.SetSave` — clones the hardcore flag GameObject, renames it to `"lblSoftcore"`, destroys `UILocalize` components, sets the text to `"Softcore"` or `"Softcore N"` (where N is `GetPermanentDeathCount(uid)`), sets the color to purple `#A855F7`, and hides the original hardcore icon.
- **Save instance label:** Harmony Postfix on `CharacterSaveInstanceDisplay.SetSaveInstance` — checks `IsRestoredBackupInstance(uid, instancePath)` (tests if the instance directory exists under the backups path). If true, it saves the original `lblAreaName` RectTransform layout, repositions it to top-right, creates a `lblSaveSoftcore` clone from the `lblTime` template, positions it at bottom-right, and sets its text to "Backup" in purple. If false, it restores the original layout and hides the label.
- **Backup path check:** `BepInEx/config/gymmed.Mods_Communicator/Softcore_Mode/Backups/{uid}/{instancePath}/` — if this directory exists, the instance is a backup.

</details>
</details>

<details>
<summary><b>Split-Screen Support</b> — softcore mode works with 2-player split-screen. Both players can play in softcore mode together.</summary>

In split-screen, each player's softcore status is tracked independently via per-character metadata files. Both players can select softcore mode during character creation. Each character's death behavior follows their own mode. When either player clicks the "Save Backup" button in the pause menu, both players' saves are backed up at once. Each character tracks its own backup cooldown independently. The character selection panel correctly shows softcore characters as selectable regardless of their difficulty mode.

<details>
<summary><b>Technical Details</b></summary>

- **Character creation:** Uses `PendingSoftcoreCount` (set to 1 for single-player, 2 for split-screen by `Patch_MainScreen_DifficultySelection`) to ensure every new softcore character gets metadata XML written by `Patch_SaveManager_ConfirmAddNewSave`.
- **Per-character checks:** The defeat patch uses per-character `IsSoftcoreCharacter(uid)` instead of the global `IsCurrentGameSoftcore` flag. All softcore characters share a single death roll.
- **Selection panel fix:** Harmony Postfix on `CharacterSelectionPanel.RefreshCharacterList` forces softcore save slots to be `interactable = true` on non-primary player panels (`PlayerID > 0`), even when `slot.HarcoreMode` disagrees with `CharacterManager.Instance.HardcoreMode`. This prevents the vanilla filter from greying out softcore characters.
- **Known limitation:** Global death deletion — when `HardcoreDeathTriggered` is set, all local saves are deleted (same as vanilla hardcore).

</details>
</details>

<details>
<summary><b>Backup Cooldown & Limits</b> — configurable in-game time cooldown between manual backups and automatic pruning of old backups.</summary>

The backup cooldown uses in-game time (tracked by the game's clock), not real-world time, so it only counts time spent playing with the save loaded. The static cooldown defaults to 24 in-game hours and can be set to 0 to disable it entirely. A cooldown randomizer can be enabled to roll a random cooldown between a configurable min and max (default 24–168 hours) each time a backup is created. For characters restored from backup, the cooldown initialization is deferred until the game time becomes valid (after scene loading completes). The backup limit (default 10) automatically removes the oldest backup instances when exceeded.

<details>
<summary>Save Backup button in the pause menu, colored purple</summary>

<img src="https://raw.githubusercontent.com/GymMed/Outward-Softcore-Mode/refs/heads/main/preview/images/PauseSettings-Save_Backup.png" alt="Save Backup button in the pause menu"/>
</details>

<details>
<summary>Save Backup button showing the cooldown timer</summary>

<img src="https://raw.githubusercontent.com/GymMed/Outward-Softcore-Mode/refs/heads/main/preview/images/PauseSettings-Save_Backup_Timer.png" alt="Save Backup button showing cooldown timer"/>
</details>

<details>
<summary><b>Technical Details</b></summary>

- **Cooldown storage:** `LastBackupGameTime` field in metadata XML at `BepInEx/config/gymmed.Mods_Communicator/Softcore_Mode/Characters/{UID}.xml`. Uses `EnvironmentConditions.GameTimeF` (in-game float time).
- **Cooldown check:** `CanBackupNow(uid)` returns `true` if `(currentGameTime - lastBackupTime) >= storedCooldownDuration` or if cooldown is 0 (disabled). Falls back to `SaveCooldownHours` config when no per-character duration is stored.
- **Cooldown randomizer:** When `CooldownRandomizerEnabled` is true, each backup rolls `Random.Range(CooldownRandomizerMinHours, CooldownRandomizerMaxHours)` and stores the result as `CooldownDuration` in the character metadata XML.
- **Deferred initialization for restored characters:** When a restored character is loaded (`Patch_Character_LoadPlayerSave`), the UID is added to `PendingCooldownUIDs`. In `OutwardSoftcoreMode.Update()`, once `EnvironmentConditions.GameTimeF > 0f`, the cooldown is initialized and the restored flag is cleared.
- **Backup limit enforcement:** `EnforceBackupLimit(uid)` sorts backup directories by name descending, deletes the oldest ones exceeding `MaxBackups`. Each backup is a full timestamp directory under `BepInEx/config/gymmed.Mods_Communicator/Softcore_Mode/Backups/{uid}/`.
- **Config:** `SaveCooldownHours` (float, 0-any, default 24), `CooldownRandomizerEnabled` (bool, default true), `CooldownRandomizerMinHours` (float, 0-any, default 24), `CooldownRandomizerMaxHours` (float, 0-any, default 168), `MaxBackups` (int, 1-any, default 10).

</details>
</details>

## Publish/Subscribe

This mod uses [Mods Communicator](https://github.com/GymMed/Outward-Mods-Communicator) for event-driven communication between mods.

### Publish

These events are published by Softcore Mode. Other mods can subscribe to them to react to in-game events.

<details>
<summary>SaveBackupBefore</summary>

Fired before a manual backup is created. The `callerUID` parameter contains the UID of the character who triggered the save.

```csharp
EventBus.Subscribe("gymmed.softcore_mode_*", "SaveBackupBefore", payload =>
{
    string callerUID = payload.Get<string>("callerUID", null);
    if (string.IsNullOrEmpty(callerUID)) return;
    // Your code here
});
```
</details>

<details>
<summary>SaveBackupAfter</summary>

Fired after a manual backup is created. The `callerUID` parameter contains the UID of the backed-up character.

```csharp
EventBus.Subscribe("gymmed.softcore_mode_*", "SaveBackupAfter", payload =>
{
    string callerUID = payload.Get<string>("callerUID", null);
    if (string.IsNullOrEmpty(callerUID)) return;
    // Your code here
});
```
</details>

<details>
<summary>DeathRollBefore</summary>

Fired before the permanent death roll is made. The `softcoreUIDs` parameter contains the list of UIDs for all softcore characters at 0 HP.

```csharp
EventBus.Subscribe("gymmed.softcore_mode_*", "DeathRollBefore", payload =>
{
    var uids = payload.Get<List<string>>("softcoreUIDs", null);
    if (uids == null || uids.Count == 0) return;
    // Your code here
});
```
</details>

<details>
<summary>DeathRollAfter</summary>

Fired after the death roll is made. The `rollResult` parameter indicates if death was triggered, and `affectedUIDs` lists the characters whose death count was incremented.

```csharp
EventBus.Subscribe("gymmed.softcore_mode_*", "DeathRollAfter", payload =>
{
    bool deathTriggered = payload.Get<bool>("rollResult", false);
    var affectedUIDs = payload.Get<List<string>>("affectedUIDs", null);
    // Your code here
});
```
</details>

### Subscribe

Softcore Mode does not subscribe to external events. All events listed above are published for other mods to subscribe to.

## Configuration

All settings are in `BepInEx/config/gymmed.softcore_mode.cfg` and can be edited with any text editor or through r2modman's config editor.

| Setting                     | Type         | Default | Description                                                                             |
| --------------------------- | ------------ | ------- | --------------------------------------------------------------------------------------- |
| `DeathChance`               | int (20–100) | 20      | Permanent death chance on defeat (%)                                                    |
| `MaxBackups`                | int          | 10      | Max backup instances kept per character. Oldest are auto-deleted when limit is exceeded |
| `SaveCooldownHours`         | float        | 24.0    | Minimum in-game hours between manual backups. Set to 0 to disable cooldown              |
| `CooldownRandomizerEnabled` | bool         | true    | Enables random cooldown range for backup saves                                          |
| `CooldownRandomizerMinHours`| float        | 24.0    | Minimum random cooldown in game hours                                                   |
| `CooldownRandomizerMaxHours`| float        | 168.0   | Maximum random cooldown in game hours                                                   |

## Data Storage

All mod data is stored under `BepInEx/config/gymmed.Mods_Communicator/Softcore_Mode/`. The config file itself is at `BepInEx/config/gymmed.softcore_mode.cfg`.

| Data                                                  | Path                                                                   |
| ----------------------------------------------------- | ---------------------------------------------------------------------- |
| Character metadata (softcore flag, name, death count) | `Softcore_Mode/Characters/{UID}.xml`                                   |
| Backup instances (full save copies)                   | `Softcore_Mode/Backups/{UID}/{instanceTimestamp}/`                     |
| Backup instance game time                             | `Softcore_Mode/Backups/{UID}/{instanceTimestamp}/SoftcoreSaveData.xml` |

## Installation

### Manual

To manually set up, do the following

1. Create the directory: `Game\BepInEx\plugins\OutwardSoftcoreMode\`.
2. Extract the archive into any directory(recommend empty).
3. Move the contents of the plugins\ directory from the archive into the `BepInEx\plugins\OutwardSoftcoreMode\` directory you created.
4. It should look like `Game\BepInEx\plugins\OutwardSoftcoreMode\OutwardSoftcoreMode.dll`.
5. Ensure all dependencies are installed.
6. Launch the game.

### r2modman

1. Open r2modman and select your Outward profile.
2. Go to the **Online** tab and search for "Softcore Mode".
3. Click **Download** and ensure dependencies are selected.
4. Launch the game from r2modman.

### If you liked the mod leave a star on [GitHub](https://github.com/GymMed/Outward-Softcore-Mode) it's free
