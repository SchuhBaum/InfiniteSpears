using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using System;
using System.Reflection;
using UnityEngine;
using static AbstractPhysicalObject;
using static InfiniteSpears.AbstractPlayerMod;
using static InfiniteSpears.MainMod;
using static MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName;

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

        if (Type.GetType("Player, Assembly-CSharp") is Type player_class) {
            try {
                // don't put slug to back when you have backspears;
                new Hook(player_class.GetProperty("CanPutSlugToBack", BindingFlags.Public | BindingFlags.Instance).GetMethod, typeof(PlayerMod).GetMethod("Player_CanPutSlugToBack"));
            } catch (Exception exception) {
                Debug.Log("InfiniteSpears: " + exception);
            }
        } else {
            Debug.Log("InfiniteSpears: Failed to create property hooks for class Player.");
        }
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

    public static bool Player_CanPutSlugToBack(Func<Player, bool> orig, Player player) {
        bool vanilla_result = orig(player);
        if (player.Get_Attached_Fields().abstract_on_back_sticks.Count > 0) return false;
        return vanilla_result;
    }

    public static bool Uses_A_Persistant_Tracker(AbstractPhysicalObject abstract_physical_object) {
        //
        // copy & paste vanilla function but it works with "key item tracking" disabled
        // as well;
        //

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
                if (spear_on_back.abstractStick == null && spear_on_back.spear == null) return true;

                // don't check attached_fields.is_blacklisted since you might have the backspear 
                // perk active; prioritize spawning spears from backspears;
                if (player.Get_Attached_Fields().has_infinite_spears) return false;
                return player.grasps[0] == null || player.grasps[1] == null;
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
                if (spear_on_back.abstractStick != null) return;
                if (spear_on_back.spear != null) return;

                spear_on_back.abstractStick = new AbstractOnBackStick(player.abstractPhysicalObject, abstract_spear);
                spear_on_back.spear = (Spear)spear_on_back.abstractStick.Spear.realizedObject; // null is okay;
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

    private static void Player_Ctor(On.Player.orig_ctor orig, Player player, AbstractCreature abstract_player, World world) {
        orig(player, abstract_player, world);

        // return when this is already initialized; otherwise this can conflict with the
        // swallow everything mod;
        if (_all_attached_fields.ContainsKey(abstract_player)) return;

        if (player.SlugCatClass is not Name slugcat_name) {
            _all_attached_fields.Add(abstract_player, new(max_spear_count: 0));
            return;
        }

        if (slugcat_name == Yellow) {
            _all_attached_fields.Add(abstract_player, new(Option_Max_Spear_Count_Yellow));
        } else if (slugcat_name == White) {
            _all_attached_fields.Add(abstract_player, new(Option_Max_Spear_Count_White));
        } else if (slugcat_name == Red) {
            _all_attached_fields.Add(abstract_player, new(Option_Max_Spear_Count_Red));
        } else if (ModManager.MSC) {
            if (slugcat_name == Gourmand) {
                _all_attached_fields.Add(abstract_player, new(Option_Max_Spear_Count_Gourmand));
            } else if (slugcat_name == Artificer) {
                _all_attached_fields.Add(abstract_player, new(Option_Max_Spear_Count_Artificer));
            } else if (slugcat_name == Rivulet) {
                _all_attached_fields.Add(abstract_player, new(Option_Max_Spear_Count_Rivulet));
            } else if (slugcat_name == MoreSlugcatsEnums.SlugcatStatsName.Spear) {
                _all_attached_fields.Add(abstract_player, new(Option_Max_Spear_Count_Spearmaster));
            } else if (slugcat_name == Saint) {
                _all_attached_fields.Add(abstract_player, new(Option_Max_Spear_Count_Saint));
            } else if (slugcat_name == Sofanthiel) {
                _all_attached_fields.Add(abstract_player, new(Option_Max_Spear_Count_Sofanthiel));
            } else {
                _all_attached_fields.Add(abstract_player, new(Option_Max_Spear_Count_Custom_Slugcats));
            }
        } else {
            _all_attached_fields.Add(abstract_player, new(Option_Max_Spear_Count_Custom_Slugcats));
        }

        if (player.Get_Attached_Fields().max_spear_count == 0) return;
        player.spearOnBack = new SpearOnBack(player);
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
