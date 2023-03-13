using UnityEngine;

namespace InfiniteSpears;

internal static class RainWorldGameMod
{
    internal static void OnEnable()
    {
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
    }

    // ----------------- //
    // private functions //
    // ----------------- //

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame game, ProcessManager manager)
    {
        Debug.Log("InfiniteSpears: Initialize variables.");

        AbstractPlayerMod.all_attached_fields.Clear();
        JokeRifleMod.OnToggle();
        PlayerMod.OnToggle();

        orig(game, manager);
    }

    private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame game)
    {
        Debug.Log("InfiniteSpears: Cleanup.");

        orig(game);

        AbstractPlayerMod.all_attached_fields.Clear();
        JokeRifleMod.OnToggle();
        PlayerMod.OnToggle();
    }
}