using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using UnityEngine;

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
            IL.Player.GrabUpdate += IL_Player_GrabUpdate;

            On.Player.ctor += Player_ctor; // create list of backspears
            On.Player.Die += Player_Die; // drop all backspears
            On.Player.Stun += Player_Stun; // drop all backspears
        }

        //
        // private
        //

        private static void IL_Player_GrabUpdate(ILContext context)
        {
            ILCursor cursor = new(context);
            // MainMod.LogAllInstructions(context);

            cursor.TryGotoNext(instruction => instruction.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>("Spear"));
            cursor.TryGotoNext(instruction => instruction.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>("Spear"));
            cursor.TryGotoNext(instruction => instruction.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>("Spear"));

            // allow needles to be created when there is space on the back
            if (cursor.TryGotoNext(instruction => instruction.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>("Spear")))
            {
                Debug.Log("InfiniteSpears: IL_Player_GrabUpdate_1: Index " + cursor.Index); // 597
                cursor.Goto(cursor.Index + 4);
                cursor.RemoveRange(8); // 601-608
                cursor.Next.OpCode = OpCodes.Brfalse;

                cursor.EmitDelegate<Func<Player, bool>>(player =>
                {
                    AbstractPlayerMod.AttachedFields attachedFields = player.abstractCreature.GetAttachedFields();
                    if (attachedFields.isBlacklisted || player.spearOnBack is not Player.SpearOnBack spearOnBack)
                    {
                        // vanilla case
                        return player.grasps[0] == null || player.grasps[1] == null;
                    }

                    if (spearOnBack.spear != null)
                    {
                        // priotize spawning spears from backspears;
                        if (MainMod.Option_MaxSpearCount == 1) return false;
                        return player.grasps[0] == null || player.grasps[1] == null;
                    }

                    // abstractStick is not used when Option_MaxSpearCount > 1
                    if (MainMod.Option_MaxSpearCount == 1 && spearOnBack.abstractStick != null)
                    {
                        return false;
                    }
                    return true;
                });
            }
            else
            {
                Debug.LogException(new Exception("InfiniteSpears: IL_Player_GrabUpdate_1 failed."));
                return;
            }

            // allows needles to be put on the back instead of being dropped when hands are full
            if (cursor.TryGotoNext(instruction => instruction.MatchCall<Player>("FreeHand")))
            {
                Debug.Log("InfiniteSpears: IL_Player_GrabUpdate_2: Index " + cursor.Index); // 957 // 952
                cursor = cursor.RemoveRange(4); // 957-960

                // cursor.GotoNext() didn't work for some reason;
                cursor.Goto(cursor.Index + 1);
                cursor.RemoveRange(4); // 962-965

                cursor.EmitDelegate<Action<Player, AbstractSpear>>((player, abstractSpear) =>
                {
                    // vanilla case
                    if (player.FreeHand() > -1)
                    {
                        player.SlugcatGrab(abstractSpear.realizedObject, player.FreeHand());
                        return;
                    }

                    AbstractPlayerMod.AttachedFields attachedFields = player.abstractCreature.GetAttachedFields();
                    if (attachedFields.isBlacklisted) return;
                    if (player.spearOnBack is not Player.SpearOnBack spearOnBack) return;
                    if (spearOnBack.spear != null) return;

                    // abstractStick is not consistently updated if number of backspears > 1;
                    // otherwise this can lead to the situation that you have room on your back
                    // but you still can't spawn needles;
                    if (MainMod.Option_MaxSpearCount == 1 && spearOnBack.abstractStick != null) return;

                    spearOnBack.abstractStick = new Player.AbstractOnBackStick(player.abstractPhysicalObject, abstractSpear);
                    spearOnBack.spear = (Spear)abstractSpear.realizedObject; // null is okay;
                    spearOnBack.interactionLocked = true;
                    player.noPickUpOnRelease = 20;

                    if (abstractSpear.realizedObject is not Spear spear) return;
                    spear.ChangeMode(Weapon.Mode.OnBack);
                });
            }
            else
            {
                Debug.LogException(new Exception("InfiniteSpears: IL_Player_GrabUpdate_2 failed."));
                return;
            }
            // MainMod.LogAllInstructions(context);
        }

        //
        //
        //

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