using System.Security.Permissions;
using BepInEx;
using UnityEngine;

// temporary fix // should be added automatically //TODO
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace InfiniteSpears
{
    [BepInPlugin("SchuhBaum.InfiniteSpears", "InfiniteSpears", "2.0.1")]
    public class MainMod : BaseUnityPlugin
    {
        //
        // meta data
        //

        public static readonly string MOD_ID = "InfiniteSpears";
        public static readonly string author = "SchuhBaum";
        public static readonly string version = "2.0.1";

        //
        // options
        //

        public static int Option_MaxSpearCount => MainModOptions.maxSpearCountSlider.Value;

        //
        // variables
        //

        public static bool isInitialized = false;

        // 
        // main
        // 

        public MainMod() { }
        public void OnEnable() => On.RainWorld.OnModsInit += RainWorld_OnModsInit;// look for dependencies and initialize hooks

        //
        // private
        //

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld rainWorld)
        {
            orig(rainWorld);

            MachineConnector.SetRegisteredOI(MOD_ID, MainModOptions.instance);

            if (isInitialized) return;
            isInitialized = true;

            Debug.Log("InfiniteSpears: version " + version);

            AbstractObjectStickMod.OnEnable();
            PlayerCarryableItemMod.OnEnable();
            PlayerMod.OnEnable();
            ShortcutHelperMod.OnEnable();

            SpearMod.OnEnable();
            SpearOnBackMod.OnEnable();
            WeaponMod.OnEnable();
        }
    }
}