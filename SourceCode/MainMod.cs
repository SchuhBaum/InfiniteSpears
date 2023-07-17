using BepInEx;
using MonoMod.Cil;
using System.Security.Permissions;
using UnityEngine;
using static InfiniteSpears.MainModOptions;

// allows access to private members;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace InfiniteSpears;

[BepInPlugin("SchuhBaum.InfiniteSpears", "InfiniteSpears", "2.1.6")]
public class MainMod : BaseUnityPlugin {
    //
    // meta data
    //

    public static readonly string mod_id = "InfiniteSpears";
    public static readonly string author = "SchuhBaum";
    public static readonly string version = "2.1.6";

    //
    // options
    //

    public static bool Option_JokeRifle => joke_rifle.Value;
    public static bool Option_SwallowedItems => swallowed_items.Value;

    public static int Option_Max_Spear_Count_Yellow => max_spear_count_slider_yellow.Value;
    public static int Option_Max_Spear_Count_White => max_spear_count_slider_white.Value;
    public static int Option_Max_Spear_Count_Red => max_spear_count_slider_red.Value;

    public static int Option_Max_Spear_Count_Gourmand => max_spear_count_slider_gourmand.Value;
    public static int Option_Max_Spear_Count_Artificer => max_spear_count_slider_artificer.Value;
    public static int Option_Max_Spear_Count_Rivulet => max_spear_count_slider_rivulet.Value;
    public static int Option_Max_Spear_Count_Spearmaster => max_spear_count_slider_spearmaster.Value;
    public static int Option_Max_Spear_Count_Saint => max_spear_count_slider_saint.Value;

    public static int Option_Max_Spear_Count_Sofanthiel => max_spear_count_slider_sofanthiel.Value;
    public static int Option_Max_Spear_Count_Custom_Slugcats => max_spear_count_slider_custom_slugcats.Value;

    //
    // variables
    //

    public static bool is_initialized = false;
    public static bool can_log_il_hooks = false;

    // 
    // main
    // 

    public MainMod() { }
    public void OnEnable() => On.RainWorld.OnModsInit += RainWorld_OnModsInit; // look for dependencies and initialize hooks

    //
    // public
    //

    public static void LogAllInstructions(ILContext? context, int index_string_length = 9, int op_code_string_length = 14) {
        if (context == null) return;

        Debug.Log("-----------------------------------------------------------------");
        Debug.Log("Log all IL-instructions.");
        Debug.Log("Index:" + new string(' ', index_string_length - 6) + "OpCode:" + new string(' ', op_code_string_length - 7) + "Operand:");

        ILCursor cursor = new(context);
        ILCursor label_cursor = cursor.Clone();

        string cursor_index_string;
        string op_code_string;
        string operand_string;

        while (true) {
            // this might return too early;
            // if (cursor.Next.MatchRet()) break;

            // should always break at some point;
            // only TryGotoNext() doesn't seem to be enough;
            // it still throws an exception;
            try {
                if (cursor.TryGotoNext(MoveType.Before)) {
                    cursor_index_string = cursor.Index.ToString();
                    cursor_index_string = cursor_index_string.Length < index_string_length ? cursor_index_string + new string(' ', index_string_length - cursor_index_string.Length) : cursor_index_string;
                    op_code_string = cursor.Next.OpCode.ToString();

                    if (cursor.Next.Operand is ILLabel label) {
                        label_cursor.GotoLabel(label);
                        operand_string = "Label >>> " + label_cursor.Index;
                    } else {
                        operand_string = cursor.Next.Operand?.ToString() ?? "";
                    }

                    if (operand_string == "") {
                        Debug.Log(cursor_index_string + op_code_string);
                    } else {
                        op_code_string = op_code_string.Length < op_code_string_length ? op_code_string + new string(' ', op_code_string_length - op_code_string.Length) : op_code_string;
                        Debug.Log(cursor_index_string + op_code_string + operand_string);
                    }
                } else {
                    break;
                }
            } catch {
                break;
            }
        }
        Debug.Log("-----------------------------------------------------------------");
    }

    //
    // private
    //

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld rain_world) {
        orig(rain_world);
        MachineConnector.SetRegisteredOI(mod_id, main_mod_options);

        if (is_initialized) return;
        is_initialized = true;

        Debug.Log("InfiniteSpears: version " + version);

        can_log_il_hooks = true;
        AbstractObjectStickMod.OnEnable();
        PlayerCarryableItemMod.OnEnable();
        PlayerMod.OnEnable();

        RainWorldGameMod.OnEnable();
        ProcessManagerMod.OnEnable();
        ShortcutHelperMod.OnEnable();
        SpearMod.OnEnable();

        SpearOnBackMod.OnEnable();
        WeaponMod.OnEnable();
        can_log_il_hooks = false;
    }
}
