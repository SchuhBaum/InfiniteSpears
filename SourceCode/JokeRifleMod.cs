using static InfiniteSpears.MainMod;

namespace InfiniteSpears;

internal class JokeRifleMod
{
    //
    // variables
    //

    private static bool is_enabled = false;

    //
    //
    //

    internal static void OnToggle()
    {
        is_enabled = !is_enabled;
        if (Option_JokeRifle)
        {
            if (is_enabled)
            {
                On.JokeRifle.AbstractRifle.setCurrentAmmo += JokeRifle_AbstractRifle_SetCurrentAmmo;
            }
            else
            {
                On.JokeRifle.AbstractRifle.setCurrentAmmo -= JokeRifle_AbstractRifle_SetCurrentAmmo;
            }
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