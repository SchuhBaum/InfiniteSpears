using System.Collections.Generic;

namespace InfiniteSpears
{
    internal static class PlayerMod
    {
        internal static void OnEnable()
        {
            On.Player.ctor += Player_ctor; // create list of backspears
            On.Player.Die += Player_Die; // drop all backspears
            On.Player.Stun += Player_Stun; // drop all backspears
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void Player_ctor(On.Player.orig_ctor orig, Player player, AbstractCreature abstractCreature, World world)
        {
            orig(player, abstractCreature, world);
            player.spearOnBack = new Player.SpearOnBack(player); // all characters can carry spears on their back

            int playerNumber = player.playerState.playerNumber;
            SpearOnBackMod.abstractOnBackSticks[playerNumber]?.Clear();
            SpearOnBackMod.abstractOnBackSticks[playerNumber] = new List<Player.AbstractOnBackStick>();
        }

        private static void Player_Die(On.Player.orig_Die orig, Player player)
        {
            if (player.spearOnBack != null)
            {
                SpearOnBackMod.DropAllSpears(player.spearOnBack);
            }
            orig(player);
        }

        private static void Player_Stun(On.Player.orig_Stun orig, Player player, int stun)
        {
            if (stun > 80 && player.spearOnBack != null)
            {
                SpearOnBackMod.DropAllSpears(player.spearOnBack);
            }
            orig(player, stun);
        }
    }
}