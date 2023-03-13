using System.Security.Permissions;
using BepInEx;
using MonoMod.Cil;
using UnityEngine;

using static InfiniteSpears.MainModOptions;

// temporary fix // should be added automatically //TODO
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace InfiniteSpears;

[BepInPlugin("SchuhBaum.InfiniteSpears", "InfiniteSpears", "2.1.0")]
public class MainMod : BaseUnityPlugin
{
    //
    // meta data
    //

    public static readonly string MOD_ID = "InfiniteSpears";
    public static readonly string author = "SchuhBaum";
    public static readonly string version = "2.1.0";

    //
    // options
    //

    public static int Option_MaxSpearCount => maxSpearCountSlider.Value;

    public static bool Option_Yellow => includeYellow.Value;
    public static bool Option_White => includeWhite.Value;
    public static bool Option_Red => includeRed.Value;

    public static bool Option_Gourmand => includeGourmand.Value;
    public static bool Option_Artificer => includeArtificer.Value;
    public static bool Option_Rivulet => includeRivulet.Value;
    public static bool Option_Spearmaster => includeSpearmaster.Value;
    public static bool Option_Saint => includeSaint.Value;

    public static bool Option_JokeRifle => joke_rifle.Value;
    public static bool Option_SwallowedItems => swallowed_items.Value;

    //
    // variables
    //

    public static bool is_initialized = false;

    // 
    // main
    // 

    public MainMod() { }
    public void OnEnable() => On.RainWorld.OnModsInit += RainWorld_OnModsInit; // look for dependencies and initialize hooks

    //
    // public
    //

    public static void LogAllInstructions(ILContext? context, int indexStringLength = 9, int opCodeStringLength = 14)
    {
        if (context == null) return;

        Debug.Log("-----------------------------------------------------------------");
        Debug.Log("Log all IL-instructions.");
        Debug.Log("Index:" + new string(' ', indexStringLength - 6) + "OpCode:" + new string(' ', opCodeStringLength - 7) + "Operand:");

        ILCursor cursor = new(context);
        ILCursor labelCursor = cursor.Clone();

        string cursorIndexString;
        string opCodeString;
        string operandString;

        while (true)
        {
            // this might return too early;
            // if (cursor.Next.MatchRet()) break;

            // should always break at some point;
            // only TryGotoNext() doesn't seem to be enough;
            // it still throws an exception;
            try
            {
                if (cursor.TryGotoNext(MoveType.Before))
                {
                    cursorIndexString = cursor.Index.ToString();
                    cursorIndexString = cursorIndexString.Length < indexStringLength ? cursorIndexString + new string(' ', indexStringLength - cursorIndexString.Length) : cursorIndexString;
                    opCodeString = cursor.Next.OpCode.ToString();

                    if (cursor.Next.Operand is ILLabel label)
                    {
                        labelCursor.GotoLabel(label);
                        operandString = "Label >>> " + labelCursor.Index;
                    }
                    else
                    {
                        operandString = cursor.Next.Operand?.ToString() ?? "";
                    }

                    if (operandString == "")
                    {
                        Debug.Log(cursorIndexString + opCodeString);
                    }
                    else
                    {
                        opCodeString = opCodeString.Length < opCodeStringLength ? opCodeString + new string(' ', opCodeStringLength - opCodeString.Length) : opCodeString;
                        Debug.Log(cursorIndexString + opCodeString + operandString);
                    }
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        }
        Debug.Log("-----------------------------------------------------------------");
    }

    //
    // private
    //

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld rainWorld)
    {
        orig(rainWorld);

        MachineConnector.SetRegisteredOI(MOD_ID, instance);

        if (is_initialized) return;
        is_initialized = true;

        Debug.Log("InfiniteSpears: version " + version);

        AbstractObjectStickMod.OnEnable();
        PlayerCarryableItemMod.OnEnable();
        PlayerMod.OnEnable();
        RainWorldGameMod.OnEnable();

        ShortcutHelperMod.OnEnable();
        SpearMod.OnEnable();
        SpearOnBackMod.OnEnable();
        WeaponMod.OnEnable();
    }
}