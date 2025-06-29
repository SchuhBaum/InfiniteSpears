using UnityEngine;
using static InfiniteSpears.MainMod;
using static InfiniteSpears.MainModOptions;
using static ProcessManager;

namespace InfiniteSpears;

public static class ProcessManagerMod {
    //
    // main
    //

    internal static void OnEnable() {
        On.ProcessManager.RequestMainProcessSwitch_ProcessID += ProcessManager_RequestMainProcessSwitch;
    }

    //
    // public
    //

    public static void Initialize_Option_Specific_Hooks() {
        // without can_log_il_hooks the logs are repeated
        // for every other mod adding the corresponding IL hook;

        main_mod_options.Log_All_Options();
        Debug.Log("InfiniteSpears: Initialize option specific hooks.");

        can_log_il_hooks = true;
        JokeRifleMod.On_Config_Changed();
        PlayerMod.On_Config_Changed();
        can_log_il_hooks = false;
    }

    //
    // private
    //

    private static void ProcessManager_RequestMainProcessSwitch(On.ProcessManager.orig_RequestMainProcessSwitch_ProcessID orig, ProcessManager process_manager, ProcessID next_process_id) {
        // I want to use the event OnConfigChanged in MainModOptions;
        // but I had cases from other users where logging was not triggered
        // when starting the game;
        // it seems to be inconsistent otherwise;
        // maybe I can do both and use this as a backup;
        // from what I tested this is triggered after the event OnConfigChanged;

        ProcessID current_process_id = process_manager.currentMainLoop.ID;
        orig(process_manager, next_process_id);

        if (current_process_id == ProcessID.Initialization) {
            // Doing this inside MainMod.PostModsInit() does not work reliably.
            foreach (var entry in SlugcatStats.Name.values.entries) {
                var name = Regex.Replace(entry, @"[^a-zA-Z0-9_]", "_");
                if (blacklisted_custom_slugcat_names.Contains(name)) {
                    continue;
                }

                var configurable = main_mod_options.config.Bind($"max_spear_count_slider_custom_slugcat_{name}", defaultValue: 0, new ConfigurableInfo("For values X > 0, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(-1, 7), "", $"Number of BackSpears for {name} (0)"));
                max_spear_count_slider_custom_slugcats.Add(configurable);
            }

            Initialize_Option_Specific_Hooks();
        }
    }
}
