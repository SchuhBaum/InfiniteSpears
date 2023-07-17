using UnityEngine;

namespace InfiniteSpears;

internal static class RainWorldGameMod {
    //
    // main
    //

    internal static void OnEnable() {
        On.RainWorldGame.ctor += RainWorldGame_Ctor;
        On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
    }

    //
    // private
    //

    private static void RainWorldGame_Ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame game, ProcessManager manager) {
        Debug.Log("InfiniteSpears: Initialize variables.");
        AbstractPlayerMod.all_attached_fields.Clear();
        orig(game, manager);
    }

    private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame game) {
        Debug.Log("InfiniteSpears: Cleanup.");
        orig(game);
        AbstractPlayerMod.all_attached_fields.Clear();
    }
}
