using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using UnityEngine;
using static AbstractPhysicalObject;
using static InfiniteSpears.AbstractPlayerMod;
using static InfiniteSpears.MainMod;
using static Player;
using static SlugcatStats;
using static SlugcatStats.Name;

namespace InfiniteSpears;

public static class PlayerMod {
    //
    // variables
    //

    public static Attached_Fields Get_Attached_Fields(this Player player) => player.abstractCreature.Get_Attached_Fields();

    //
    // main
    //

    internal static void OnEnable() {
        IL.Player.GrabUpdate += IL_Player_GrabUpdate;

        On.Player.ctor += Player_Ctor; // create list of backspears
        On.Player.Die += Player_Die; // drop all backspears
        On.Player.Stun += Player_Stun; // drop all backspears
    }

    internal static void On_Config_Changed() {
        On.Player.Regurgitate -= Player_Regurgitate;

        if (Option_SwallowedItems) {
            On.Player.Regurgitate += Player_Regurgitate;
        }
    }

    //
    // public
    //

    public static bool Uses_A_Persistant_Tracker(AbstractPhysicalObject abstract_physical_object) {
        if (abstract_physical_object.type == AbstractObjectType.NSHSwarmer) return true;
        if (abstract_physical_object.type == MoreSlugcatsEnums.AbstractObjectType.EnergyCell) return true;
        if (abstract_physical_object.type == MoreSlugcatsEnums.AbstractObjectType.JokeRifle) return true;

        if (abstract_physical_object is DataPearl.AbstractDataPearl abstract_data_pearl && DataPearl.PearlIsNotMisc(abstract_data_pearl.dataPearlType)) return true;
        if (abstract_physical_object is VultureMask.AbstractVultureMask abstract_vulture_mask && abstract_vulture_mask.scavKing) return true;

        return false;
    }

    //
    // private
    //

    private static void IL_Player_GrabUpdate(ILContext context) {
        // LogAllInstructions(context);
        ILCursor cursor = new(context);

        // allow needles to be created when there is space on the back
        if (cursor.TryGotoNext(
              instruction => instruction.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>("Spear"),
              instruction => instruction.MatchCall("ExtEnum`1<SlugcatStats/Name>", "op_Equality"),
              instruction => instruction.MatchBrfalse(out ILLabel _),
              instruction => instruction.MatchLdarg(0),
              instruction => instruction.MatchCall<Creature>("get_grasps")
            )) {
            if (can_log_il_hooks) {
                Debug.Log("InfiniteSpears: IL_Player_GrabUpdate: Index " + cursor.Index); // 597
            }

            cursor.Goto(cursor.Index + 4);
            cursor.RemoveRange(8); // 601-608
            cursor.Next.OpCode = OpCodes.Brfalse;

            cursor.EmitDelegate<Func<Player, bool>>(player => {
                if (player.slugOnBack is SlugOnBack slug_on_back && slug_on_back.HasASlug) {
                    // vanilla case;
                    return player.grasps[0] == null || player.grasps[1] == null;
                }

                if (player.spearOnBack is not SpearOnBack spear_on_back) {
                    // vanilla case;
                    return player.grasps[0] == null || player.grasps[1] == null;
                }

                // don't check attached_fields.is_blacklisted since you might have the backspear perk
                // active;
                Attached_Fields attached_fields = player.Get_Attached_Fields();
                if (spear_on_back.spear != null) {
                    // prioritize spawning spears from backspears;
                    if (attached_fields.max_spear_count == -1) return false;
                    return player.grasps[0] == null || player.grasps[1] == null;
                }

                // abstractStick is not used when Option_MaxSpearCount > 1; (this case should not
                // matter in any case since the player is realized inside this function and 
                // spear_on_back.spear != null is already considered;)
                if (attached_fields.max_spear_count == -1 && spear_on_back.abstractStick != null) return false;
                return true;
            });
        } else {
            if (can_log_il_hooks) {
                Debug.Log("InfiniteSpears: IL_Player_GrabUpdate failed.");
            }
            return;
        }

        // allows needles to be put on the back instead of being dropped when hands are full
        if (cursor.TryGotoNext(
              instruction => instruction.MatchStfld<BodyChunk>("vel"),
              instruction => instruction.MatchLdarg(0),
              instruction => instruction.MatchCall<Player>("FreeHand")
            )) {
            if (can_log_il_hooks) {
                Debug.Log("InfiniteSpears: IL_Player_GrabUpdate: Index " + cursor.Index); // 955
            }

            cursor.Goto(cursor.Index + 2);
            cursor = cursor.RemoveRange(4); // 957-960

            // cursor.GotoNext() didn't work for some reason;
            cursor.Goto(cursor.Index + 1);
            cursor.RemoveRange(4); // 962-965

            cursor.EmitDelegate<Action<Player, AbstractSpear>>((player, abstract_spear) => {
                // vanilla case
                if (player.FreeHand() > -1) {
                    player.SlugcatGrab(abstract_spear.realizedObject, player.FreeHand());
                    return;
                }

                Attached_Fields attached_fields = player.Get_Attached_Fields();
                // if (attached_fields.isBlacklisted) return; // I might not want to check this since you can have a backspear perk as well
                if (player.spearOnBack is not SpearOnBack spear_on_back) return;
                if (spear_on_back.spear != null) return;

                // abstractStick is not consistently updated if number of backspears > 1;
                // otherwise this can lead to the situation that you have room on your back
                // but you still can't spawn needles;
                if (attached_fields.max_spear_count == -1 && spear_on_back.abstractStick != null) return;

                spear_on_back.abstractStick = new AbstractOnBackStick(player.abstractPhysicalObject, abstract_spear);
                spear_on_back.spear = (Spear)abstract_spear.realizedObject; // null is okay;
                spear_on_back.interactionLocked = true;
                player.noPickUpOnRelease = 20;

                if (abstract_spear.realizedObject is not Spear spear) return;
                spear.ChangeMode(Weapon.Mode.OnBack);
            });
        } else {
            if (can_log_il_hooks) {
                Debug.Log("InfiniteSpears: IL_Player_GrabUpdate failed.");
            }
            return;
        }
        // LogAllInstructions(context);
    }

    //
    //
    //

    private static void Player_Ctor(On.Player.orig_ctor orig, Player player, AbstractCreature abstract_creature, World world) {
        orig(player, abstract_creature, world);

        // return when this is already initialized; otherwise this can conflict with the
        // swallow everything mod;
        if (_all_attached_fields.ContainsKey(abstract_creature)) return;
        Attached_Fields attached_fields = new();
        _all_attached_fields.Add(abstract_creature, attached_fields);
        if (player.SlugCatClass is not Name slugcat_name) return;

        if (slugcat_name == Yellow) {
            if (Option_Max_Spear_Count_Yellow == 0) return;
            player.spearOnBack = new SpearOnBack(player);
            if (Option_Max_Spear_Count_Yellow == 1) return;

            attached_fields.max_spear_count = Option_Max_Spear_Count_Yellow;
            attached_fields.is_blacklisted = false;
            return;
        }

        if (slugcat_name == White) {
            if (Option_Max_Spear_Count_White == 0) return;
            player.spearOnBack = new SpearOnBack(player);
            if (Option_Max_Spear_Count_White == 1) return;

            attached_fields.max_spear_count = Option_Max_Spear_Count_White;
            attached_fields.is_blacklisted = false;
            return;
        }

        if (slugcat_name == Red) {
            if (Option_Max_Spear_Count_Red == 0) return;
            player.spearOnBack = new SpearOnBack(player);
            if (Option_Max_Spear_Count_Red == 1) return;

            attached_fields.max_spear_count = Option_Max_Spear_Count_Red;
            attached_fields.is_blacklisted = false;
            return;
        }

        if (ModManager.MSC) {
            if (slugcat_name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand) {
                if (Option_Max_Spear_Count_Gourmand == 0) return;
                player.spearOnBack = new SpearOnBack(player);
                if (Option_Max_Spear_Count_Gourmand == 1) return;

                attached_fields.max_spear_count = Option_Max_Spear_Count_Gourmand;
                attached_fields.is_blacklisted = false;
                return;
            }

            if (slugcat_name == MoreSlugcatsEnums.SlugcatStatsName.Artificer) {
                if (Option_Max_Spear_Count_Artificer == 0) return;
                player.spearOnBack = new SpearOnBack(player);
                if (Option_Max_Spear_Count_Artificer == 1) return;

                attached_fields.max_spear_count = Option_Max_Spear_Count_Artificer;
                attached_fields.is_blacklisted = false;
                return;
            }

            if (slugcat_name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet) {
                if (Option_Max_Spear_Count_Rivulet == 0) return;
                player.spearOnBack = new SpearOnBack(player);
                if (Option_Max_Spear_Count_Rivulet == 1) return;

                attached_fields.max_spear_count = Option_Max_Spear_Count_Rivulet;
                attached_fields.is_blacklisted = false;
                return;
            }

            if (slugcat_name == MoreSlugcatsEnums.SlugcatStatsName.Spear) {
                if (Option_Max_Spear_Count_Spearmaster == 0) return;
                player.spearOnBack = new SpearOnBack(player);
                if (Option_Max_Spear_Count_Spearmaster == 1) return;

                attached_fields.max_spear_count = Option_Max_Spear_Count_Spearmaster;
                attached_fields.is_blacklisted = false;
                return;
            }

            if (slugcat_name == MoreSlugcatsEnums.SlugcatStatsName.Saint) {
                if (Option_Max_Spear_Count_Saint == 0) return;
                player.spearOnBack = new SpearOnBack(player);
                if (Option_Max_Spear_Count_Saint == 1) return;

                attached_fields.max_spear_count = Option_Max_Spear_Count_Saint;
                attached_fields.is_blacklisted = false;
                return;
            }

            if (slugcat_name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel) {
                if (Option_Max_Spear_Count_Sofanthiel == 0) return;
                player.spearOnBack = new SpearOnBack(player);
                if (Option_Max_Spear_Count_Sofanthiel == 1) return;

                attached_fields.max_spear_count = Option_Max_Spear_Count_Sofanthiel;
                attached_fields.is_blacklisted = false;
                return;
            }
        }

        if (Option_Max_Spear_Count_Custom_Slugcats == 0) return;
        player.spearOnBack = new SpearOnBack(player);
        if (Option_Max_Spear_Count_Custom_Slugcats == 1) return;

        attached_fields.max_spear_count = Option_Max_Spear_Count_Custom_Slugcats;
        attached_fields.is_blacklisted = false;
    }

    private static void Player_Die(On.Player.orig_Die orig, Player player) {
        if (player.spearOnBack == null || player.Get_Attached_Fields().is_blacklisted) {
            orig(player);
            return;
        }

        SpearOnBackMod.DropAllSpears(player.spearOnBack);
        orig(player);
    }

    private static void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player player) // Option_SwallowedItems
    {
        bool hands_are_full = true;
        foreach (Creature.Grasp? grasp in player.grasps) {
            if (grasp?.grabbed == null) {
                hands_are_full = false;
                continue;
            }

            // the first / main hand should always have the two-handed objects;
            if (player.Grabability(grasp.grabbed) < ObjectGrabability.TwoHands) continue;
            hands_are_full = true;
            break;
        }

        if (hands_are_full) {
            orig(player);
            return;
        }

        if (player.objectInStomach is not AbstractPhysicalObject abstract_physical_object) {
            orig(player);
            return;
        }

        if (Uses_A_Persistant_Tracker(abstract_physical_object)) {
            orig(player);
            return;
        }

        orig(player);

        if (player.objectInStomach != null) return;
        if (abstract_physical_object is CollisionField.AbstractCollisionField) return;

        // the swallow everything mod might create crashes;
        // at least check for players;
        // slugcat npcs should have the type SlugNPC;
        if (abstract_physical_object is AbstractCreature abstract_player && abstract_player.creatureTemplate.type == CreatureTemplate.Type.Slugcat) return;
        if (abstract_physical_object is UniqueAbstractObject) return;

        // the random seed sets for example the color for slugpups;
        EntityID id = abstract_physical_object.world.game.GetNewID();
        id.altSeed = abstract_physical_object.ID.RandomSeed;

        if (abstract_physical_object is EggBugEgg.AbstractBugEgg abstract_egg_bug_egg) {
            player.objectInStomach = new EggBugEgg.AbstractBugEgg(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_egg_bug_egg.hue);
            return;
        }

        if (abstract_physical_object is FireEgg.AbstractBugEgg abstract_fire_egg) {
            player.objectInStomach = new FireEgg.AbstractBugEgg(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_fire_egg.hue);
            return;
        }

        if (abstract_physical_object is AbstractBullet abstract_bullet) {
            player.objectInStomach = new AbstractBullet(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_bullet.bulletType, abstract_bullet.timeToLive);
            return;
        }

        if (abstract_physical_object is AbstractCreature abstract_creature) {
            player.objectInStomach = new AbstractCreature(abstract_physical_object.world, abstract_creature.creatureTemplate, null, abstract_physical_object.pos, id);
            return;
        }

        if (abstract_physical_object is AbstractConsumable) {
            if (abstract_physical_object is BubbleGrass.AbstractBubbleGrass abstract_bubble_grass) {
                player.objectInStomach = new BubbleGrass.AbstractBubbleGrass(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_bubble_grass.oxygenLeft, -1, -1, null);
                return;
            }

            if (abstract_physical_object is DataPearl.AbstractDataPearl abstract_data_pearl) {
                player.objectInStomach = new DataPearl.AbstractDataPearl(abstract_physical_object.world, abstract_physical_object.type, null, abstract_physical_object.pos, id, -1, -1, null, abstract_data_pearl.dataPearlType);
                return;
            }

            if (abstract_physical_object is LillyPuck.AbstractLillyPuck abstract_lilly_puck) {
                player.objectInStomach = new LillyPuck.AbstractLillyPuck(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_lilly_puck.bites, -1, -1, null);
                return;
            }

            if (abstract_physical_object is SeedCob.AbstractSeedCob abstract_seed_cob) {
                player.objectInStomach = new SeedCob.AbstractSeedCob(abstract_physical_object.world, null, abstract_physical_object.pos, id, -1, -1, abstract_seed_cob.dead, null);
                return;
            }

            if (abstract_physical_object is SporePlant.AbstractSporePlant abstract_spore_plant) {
                player.objectInStomach = new SporePlant.AbstractSporePlant(abstract_physical_object.world, null, abstract_physical_object.pos, id, -1, -1, null, abstract_spore_plant.used, abstract_spore_plant.pacified);
                return;
            }

            if (abstract_physical_object is WaterNut.AbstractWaterNut abstract_water_nut) {
                player.objectInStomach = new WaterNut.AbstractWaterNut(abstract_physical_object.world, null, abstract_physical_object.pos, id, -1, -1, null, abstract_water_nut.swollen);
                return;
            }

            player.objectInStomach = new AbstractConsumable(abstract_physical_object.world, abstract_physical_object.type, null, abstract_physical_object.pos, id, -1, -1, null);
            return;
        }

        if (abstract_physical_object is OverseerCarcass.AbstractOverseerCarcass abstract_overseer_carcass) {
            player.objectInStomach = new OverseerCarcass.AbstractOverseerCarcass(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_overseer_carcass.color, abstract_overseer_carcass.ownerIterator);
            return;
        }

        if (abstract_physical_object is JokeRifle.AbstractRifle abstract_rifle) {
            player.objectInStomach = new JokeRifle.AbstractRifle(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_rifle.ammoStyle);
            return;
        }

        if (abstract_physical_object is AbstractSpear abstract_spear) {
            player.objectInStomach = new AbstractSpear(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_spear.explosive, abstract_spear.electric) {
                electricCharge = abstract_spear.electricCharge,
                hue = abstract_spear.hue,
                needle = abstract_spear.needle,
            };
            return;
        }

        if (abstract_physical_object is VultureMask.AbstractVultureMask abstract_vulture_mask) {
            player.objectInStomach = new VultureMask.AbstractVultureMask(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_vulture_mask.colorSeed, abstract_vulture_mask.king);
            return;
        }

        player.objectInStomach = new(abstract_physical_object.world, abstract_physical_object.type, null, abstract_physical_object.pos, id);
    }

    private static void Player_Stun(On.Player.orig_Stun orig, Player player, int stun) {
        if (player.spearOnBack == null || player.Get_Attached_Fields().is_blacklisted) {
            orig(player, stun);
            return;
        }

        if (stun <= UnityEngine.Random.Range(40, 80)) {
            orig(player, stun);
            return;
        }

        SpearOnBackMod.DropAllSpears(player.spearOnBack);
        orig(player, stun);
    }
}
