using static InfiniteSpears.MainMod;

namespace InfiniteSpears;

internal class JokeRifleMod
{
    //
    // main
    //

    internal static void On_Config_Changed()
    {
        On.JokeRifle.AbstractRifle.setCurrentAmmo -= JokeRifle_AbstractRifle_SetCurrentAmmo;

        if (Option_JokeRifle)
        {
            On.JokeRifle.AbstractRifle.setCurrentAmmo += JokeRifle_AbstractRifle_SetCurrentAmmo;
        }
    }

    //
    // private
    //

    private static void JokeRifle_AbstractRifle_SetCurrentAmmo(On.JokeRifle.AbstractRifle.orig_setCurrentAmmo orig, JokeRifle.AbstractRifle abstract_rifle, int amount) // Option_JokeRifle
    {
        if (amount <= 0) return;
        orig(abstract_rifle, 1);
    }
}