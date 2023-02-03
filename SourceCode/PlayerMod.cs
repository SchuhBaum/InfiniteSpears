using MoreSlugcats;

namespace InfiniteSpears
{
    internal static class PlayerMod
    {
        //
        // variables
        //

        public static AbstractPlayerMod.AttachedFields GetAttachedFields(this Player player) => player.abstractCreature.GetAttachedFields();

        //
        //
        //

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

            AbstractPlayerMod.allAttachedFields.Add(abstractCreature, new AbstractPlayerMod.AttachedFields());
            if (player.SlugCatClass == null) return;
            if (player.SlugCatClass == SlugcatStats.Name.Yellow && !MainMod.Option_Yellow) return;
            if (player.SlugCatClass == SlugcatStats.Name.White && !MainMod.Option_White) return;
            if (player.SlugCatClass == SlugcatStats.Name.Red && !MainMod.Option_Red) return;

            if (ModManager.MSC)
            {
                if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && !MainMod.Option_Gourmand) return;
                if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && !MainMod.Option_Artificer) return;
                if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && !MainMod.Option_Rivulet) return;
                if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear && !MainMod.Option_Spearmaster) return;
                if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && !MainMod.Option_Saint) return;
            }

            player.spearOnBack = new Player.SpearOnBack(player); // all characters can carry spears on their back
            player.GetAttachedFields().isBlacklisted = false;
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