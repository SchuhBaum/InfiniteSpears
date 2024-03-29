using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static InfiniteSpears.AbstractPlayerMod;
using static Player;

namespace InfiniteSpears;

public static class SpearOnBackMod {
    //
    // parameters
    //

    private static readonly float[] _spear_xy_modifier = new float[7] { 0.0f, -0.4f, -0.2f, -0.2f, -0.6f, -0.6f, -0.8f };
    private static readonly float[] _spear_z_modifier = new float[7] { 0.0f, 0.0f, 2f, -2f, 2f, -2f, 0.0f };

    //
    //
    //

    internal static void OnEnable() {
        On.Player.SpearOnBack.DropSpear += SpearOnBack_DropSpear;
        On.Player.SpearOnBack.GraphicsModuleUpdated += SpearOnBack_GraphicsModuleUpdated;
        On.Player.SpearOnBack.SpearToBack += SpearOnBack_SpearToBack;
        On.Player.SpearOnBack.SpearToHand += SpearOnBack_SpearToHand;
        On.Player.SpearOnBack.Update += SpearOnBack_Update;
    }

    //
    // public
    //

    public static bool CanDespawnSpear(SpearOnBack spear_on_back) {
        if (spear_on_back.interactionLocked) return false;
        if (spear_on_back.owner is not Player player) return false;
        if (!player.input[0].pckp) return false;

        if (spear_on_back.spear?.abstractPhysicalObject is not AbstractSpear carried_abstract_spear) return false;
        if (player.grasps[0]?.grabbed?.abstractPhysicalObject is not AbstractSpear held_abstract_spear) return false;

        if (carried_abstract_spear.explosive != held_abstract_spear.explosive) return false;
        if (carried_abstract_spear.electric != held_abstract_spear.electric) return false;
        if (carried_abstract_spear.needle != held_abstract_spear.needle) return false;
        if (carried_abstract_spear.hue != held_abstract_spear.hue) return false;
        return true;
    }

    public static void DespawnSpear(SpearOnBack spear_on_back) {
        if (spear_on_back.owner is not Player player) return;
        if (player.grasps[0]?.grabbed?.abstractPhysicalObject is not AbstractSpear held_abstract_spear) return;

        held_abstract_spear.realizedObject.Destroy();
        held_abstract_spear.Destroy();
        spear_on_back.interactionLocked = true;
    }

    public static void DropAllSpears(SpearOnBack spear_on_back) {
        if (spear_on_back.owner is not Player player) return;
        Attached_Fields attached_fields = player.Get_Attached_Fields();
        if (attached_fields.has_infinite_spears) return;

        for (int stick_index = attached_fields.abstract_on_back_sticks.Count - 1; stick_index >= 0; --stick_index) {
            AbstractOnBackStick abstract_on_back_stick = attached_fields.abstract_on_back_sticks[stick_index];
            if (abstract_on_back_stick.Spear.realizedObject is Spear spear) {
                spear.firstChunk.vel = player.mainBodyChunk.vel + Custom.RNV() * 3f * Random.value;
                spear.ChangeMode(Weapon.Mode.Free);
            }
            abstract_on_back_stick.Deactivate(); // removes the backspear as well
        }

        spear_on_back.abstractStick = null;
        spear_on_back.spear = null;
    }

    public static void SpawnSpear(SpearOnBack spear_on_back, in AbstractSpear? abstract_spear) {
        if (abstract_spear == null) return;
        if (spear_on_back.abstractStick != null) return;
        if (spear_on_back.spear != null) return;
        if (spear_on_back.owner is not Player player) return;

        AbstractPhysicalObject abstract_player = player.abstractPhysicalObject;
        World world = abstract_spear.world;

        if (world == null) return;

        // not sure what is random about spears;
        // but for consistency;
        EntityID id = world.game.GetNewID();
        id.altSeed = abstract_spear.ID.RandomSeed;

        AbstractSpear abstract_spear_copy = new(world, null, abstract_player.pos, id, abstract_spear.explosive, abstract_spear.electric) {
            electricCharge = abstract_spear.electricCharge,
            hue = abstract_spear.hue,
            needle = abstract_spear.needle,
        };

        abstract_spear_copy.RealizeInRoom();
        spear_on_back.abstractStick = new AbstractOnBackStick(abstract_player, abstract_spear_copy);
        spear_on_back.spear = (Spear)spear_on_back.abstractStick.Spear.realizedObject; // spear_copy;
        spear_on_back.spear?.ChangeMode(Weapon.Mode.OnBack);

        spear_on_back.interactionLocked = true;
        player.noPickUpOnRelease = 20;
        player.room?.PlaySound(SoundID.Slugcat_Pick_Up_Spear, player.mainBodyChunk);

        if (abstract_spear.realizedObject is not Spear spear) return;
        if (abstract_spear_copy.realizedObject is not Spear spear_copy) return;

        spear_copy.spearmasterNeedle = spear.spearmasterNeedle;
        spear_copy.spearmasterNeedleType = spear.spearmasterNeedleType;
        spear_copy.spearmasterNeedle_hasConnection = spear.spearmasterNeedle_hasConnection;
        spear_copy.spearmasterNeedle_fadecounter = spear.spearmasterNeedle_fadecounter;
    }

    //
    // private
    //

    private static void SpearOnBack_DropSpear(On.Player.SpearOnBack.orig_DropSpear orig, SpearOnBack spear_on_back) {
        if (spear_on_back.owner is not Player player) {
            orig(spear_on_back);
            return;
        }

        Attached_Fields attached_fields = player.Get_Attached_Fields();
        if (attached_fields.is_blacklisted || attached_fields.has_infinite_spears) {
            orig(spear_on_back);
            return;
        }

        if (spear_on_back.interactionLocked) return;
        List<AbstractOnBackStick> abstract_on_back_sticks = attached_fields.abstract_on_back_sticks;
        int current_spear_index = abstract_on_back_sticks.Count - 1;

        if (current_spear_index > -1 && abstract_on_back_sticks[current_spear_index] is AbstractOnBackStick abstract_on_back_stick) {
            if (abstract_on_back_stick.Spear.realizedObject is Spear spear) {
                spear.firstChunk.vel = player.mainBodyChunk.vel + Custom.RNV() * 3f * Random.value;
                spear.ChangeMode(Weapon.Mode.Free);
            }
            abstract_on_back_stick.Deactivate(); // removes now from abstract_on_back_sticks as well
        }

        spear_on_back.abstractStick = null;
        spear_on_back.spear = null;
        spear_on_back.interactionLocked = true;
    }

    private static void SpearOnBack_GraphicsModuleUpdated(On.Player.SpearOnBack.orig_GraphicsModuleUpdated orig, SpearOnBack spear_on_back, bool actually_viewed, bool eu) {
        if (spear_on_back.owner is not Player player) {
            orig(spear_on_back, actually_viewed, eu);
            return;
        }

        Attached_Fields attached_fields = player.Get_Attached_Fields();
        if (attached_fields.is_blacklisted) {
            orig(spear_on_back, actually_viewed, eu);
            return;
        }

        if (attached_fields.has_infinite_spears) {
            if (spear_on_back.spear?.slatedForDeletetion == true) {
                spear_on_back.spear = null; // prevents backspear from being dropped when spear gets abstracted
            }

            orig(spear_on_back, actually_viewed, eu);
            return;
        }

        List<AbstractOnBackStick> abstract_on_back_sticks = attached_fields.abstract_on_back_sticks;
        int current_spear_index = abstract_on_back_sticks.Count - 1;
        if (current_spear_index < 0) return;

        Vector2 chunk0_pos = player.mainBodyChunk.pos;
        Vector2 chunk1_pos = player.bodyChunks[1].pos;

        if (player.graphicsModule is PlayerGraphics player_graphics) {
            chunk0_pos = Vector2.Lerp(player_graphics.drawPositions[0, 0], player_graphics.head.pos, 0.2f);
            chunk1_pos = player_graphics.drawPositions[1, 0];
        }

        Vector2 body_vector = Custom.DirVec(chunk1_pos, chunk0_pos);
        bool has_gravity_and_stuff = player.Consious && player.bodyMode != BodyModeIndex.ZeroG && player.room?.gravity > 0.0;

        if (has_gravity_and_stuff) {
            spear_on_back.flip = player.bodyMode != BodyModeIndex.Default || player.animation != AnimationIndex.None || (!player.standing || player.bodyChunks[1].pos.y >= player.bodyChunks[0].pos.y - 6.0) ? (player.bodyMode != BodyModeIndex.Stand || player.input[0].x == 0 ? Custom.LerpAndTick(spear_on_back.flip, player.flipDirection * Mathf.Abs(body_vector.x), 0.15f, 1f / 6f) : Custom.LerpAndTick(spear_on_back.flip, player.input[0].x, 0.02f, 0.1f)) : Custom.LerpAndTick(spear_on_back.flip, player.input[0].x * 0.3f, 0.05f, 0.02f);
        } else {
            spear_on_back.flip = Custom.LerpAndTick(spear_on_back.flip, 0.0f, 0.15f, 1f / 7f);
        }

        for (int index = 0; index <= current_spear_index; ++index) {
            if (abstract_on_back_sticks[index].Spear.realizedObject is Spear spear && !spear.slatedForDeletetion) {
                if (has_gravity_and_stuff) {
                    if (spear_on_back.counter > 12 && !spear_on_back.interactionLocked && (player.input[0].x != 0 && player.standing)) {
                        float hand_direction = 0.0f;
                        for (int index2 = 0; index2 < player.grasps.Length; ++index2) {
                            if (player.grasps[index2] == null) {
                                hand_direction = index2 != 0 ? 1f : -1f;
                                break;
                            }
                        }
                        spear.setRotation = new Vector2?(Custom.DegToVec(Custom.AimFromOneVectorToAnother(chunk1_pos, chunk0_pos) + Custom.LerpMap(spear_on_back.counter, 12f, 20f, 0.0f, 360f * hand_direction)));
                    } else {
                        // structure: [default] - PerpendicularVector * ([rotation when standing] + [rotation when walking/crawling])
                        spear.setRotation = new Vector2?(body_vector - Custom.PerpendicularVector(body_vector) * ((1.2f + _spear_xy_modifier[index]) * (1f - Mathf.Abs(spear_on_back.flip)) - spear_on_back.flip * 0.02f * _spear_z_modifier[index]));
                        spear.ChangeOverlap(body_vector.y < -0.1 && player.bodyMode != BodyModeIndex.ClimbingOnBeam);
                    }
                } else {
                    // structure: [default] - PerpendicularVector * [rotation when standing]
                    spear.setRotation = new Vector2?(body_vector - Custom.PerpendicularVector(body_vector) * (1.2f + _spear_xy_modifier[index]));
                    spear.ChangeOverlap(false);
                }

                // structure: [height] - PerpendicularVector * [depth] // flip is about zero when standing, i.e. no depth required
                spear.firstChunk.MoveFromOutsideMyUpdate(eu, Vector2.Lerp(chunk1_pos, chunk0_pos, 0.5f - _spear_xy_modifier[index] / 4f) - Custom.PerpendicularVector(chunk1_pos, chunk0_pos) * (7.5f + _spear_z_modifier[index]) * spear_on_back.flip);
                spear.firstChunk.vel = player.mainBodyChunk.vel;
                spear.rotationSpeed = 0.0f;
            }
        }
    }

    private static void SpearOnBack_SpearToBack(On.Player.SpearOnBack.orig_SpearToBack orig, SpearOnBack spear_on_back, Spear spear) {
        if (spear_on_back.owner is not Player player) {
            orig(spear_on_back, spear);
            return;
        }

        Attached_Fields attached_fields = player.Get_Attached_Fields();
        if (attached_fields.is_blacklisted || attached_fields.has_infinite_spears) {
            orig(spear_on_back, spear);
            return;
        }

        List<AbstractOnBackStick> abstract_on_back_sticks = attached_fields.abstract_on_back_sticks;
        if (abstract_on_back_sticks.Count >= attached_fields.max_spear_count) return;

        for (int grasp = 0; grasp < 2; ++grasp) {
            if (player.grasps[grasp] != null && player.grasps[grasp].grabbed == spear) {
                player.ReleaseGrasp(grasp);
                break;
            }
        }

        spear.ChangeMode(Weapon.Mode.OnBack);
        spear_on_back.interactionLocked = true;
        player.noPickUpOnRelease = 20;
        player.room?.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, player.mainBodyChunk);

        abstract_on_back_sticks.Add(new AbstractOnBackStick(player.abstractPhysicalObject, spear.abstractPhysicalObject));
    }

    private static void SpearOnBack_SpearToHand(On.Player.SpearOnBack.orig_SpearToHand orig, SpearOnBack spear_on_back, bool eu) {
        if (spear_on_back.owner is not Player player) {
            orig(spear_on_back, eu);
            return;
        }

        Attached_Fields attached_fields = player.Get_Attached_Fields();
        if (attached_fields.is_blacklisted) {
            orig(spear_on_back, eu);
            return;
        }

        if (attached_fields.has_infinite_spears) {
            if (spear_on_back.abstractStick == null) return;
            if (spear_on_back.spear == null) return;

            // create a copy after calling orig;
            AbstractSpear abstract_spear = spear_on_back.spear.abstractSpear;
            orig(spear_on_back, eu);
            SpawnSpear(spear_on_back, abstract_spear);
            return;
        }

        List<AbstractOnBackStick> abstract_on_back_sticks = attached_fields.abstract_on_back_sticks;
        int current_spear_index = abstract_on_back_sticks.Count - 1;

        if (current_spear_index <= -1) return;
        if (abstract_on_back_sticks[current_spear_index] is not AbstractOnBackStick abstract_on_back_stick) return;
        if (abstract_on_back_stick.Spear.realizedObject is not Spear spear) return;

        foreach (Creature.Grasp? grasp in player.grasps) {
            if (grasp != null && player.Grabability(grasp.grabbed) >= ObjectGrabability.BigOneHand) return;
        }

        int grasp_used = -1;
        for (int index = 0; index < 2; ++index) {
            if (player.grasps[index] == null) {
                grasp_used = index;
                break;
            }
        }

        if (grasp_used == -1) return;

        if (player.graphicsModule is PlayerGraphics player_graphics) {
            // spear_on_back.spear.firstChunk.MoveFromOutsideMyUpdate(eu, playerGraphics.hands[graspUsed].pos);
            spear.firstChunk.MoveFromOutsideMyUpdate(eu, player_graphics.hands[grasp_used].pos);
        }

        spear.ChangeMode(Weapon.Mode.Free);
        // call Deactivate() here in case that an exception is thrown later; calls
        // abstract_on_back_sticks.Remove(abstract_on_back_stick) as well;
        abstract_on_back_stick.Deactivate();

        player.SlugcatGrab(spear, grasp_used);
        spear_on_back.interactionLocked = true;
        player.noPickUpOnRelease = 20;
        player.room?.PlaySound(SoundID.Slugcat_Pick_Up_Spear, player.mainBodyChunk);
    }

    private static void SpearOnBack_Update(On.Player.SpearOnBack.orig_Update orig, SpearOnBack spear_on_back, bool eu) {
        if (spear_on_back.owner is not Player player) {
            orig(spear_on_back, eu);
            return;
        }

        Attached_Fields attached_fields = player.Get_Attached_Fields();
        if (attached_fields.is_blacklisted) {
            orig(spear_on_back, eu);
            return;
        }

        if (attached_fields.has_infinite_spears) {
            // consistency check; was there a case where this mattered?;
            spear_on_back.spear = (Spear)spear_on_back.abstractStick?.Spear.realizedObject!;

            if (CanDespawnSpear(spear_on_back)) {
                // the animation does not match in PlayerGraphics.Update() when you have a spear 
                // on your back and slugcat's left hand is free; it tries to grab the spear with 
                // the left hand;
                ++spear_on_back.counter;
                if (spear_on_back.counter <= 20) return;
                DespawnSpear(spear_on_back);
                spear_on_back.counter = 0;
                return;
            }

            orig(spear_on_back, eu);
            return;
        }

        List<AbstractOnBackStick> abstract_on_back_sticks = attached_fields.abstract_on_back_sticks;
        int current_spear_index = abstract_on_back_sticks.Count - 1;

        // needed in case that spears are one-handed (Spearmaster);
        bool main_hand_is_empty = player.grasps[0] == null;
        bool off_hand_holds_big_item = player.grasps[1] != null && player.Grabability(player.grasps[1].grabbed) >= ObjectGrabability.BigOneHand;

        if (current_spear_index == -1) {
            spear_on_back.abstractStick = null;
            spear_on_back.spear = null;
        } else if (current_spear_index == attached_fields.max_spear_count - 1) {
            spear_on_back.abstractStick = abstract_on_back_sticks[current_spear_index];
            spear_on_back.spear = (Spear)spear_on_back.abstractStick.Spear.realizedObject;
        } else if (main_hand_is_empty && !off_hand_holds_big_item) {
            // this case is needed; otherwise it plays the wrong animation when you can dual
            // wield;
            spear_on_back.abstractStick = abstract_on_back_sticks[current_spear_index];
            spear_on_back.spear = (Spear)spear_on_back.abstractStick.Spear.realizedObject;
        } else {
            bool spear_in_hand = false;
            bool hands_are_full = true;

            // carrying another spear and getting one off the back can be possible at the same time;
            // check player hands to decide which one is the case;
            for (int index = 0; index < 2; ++index) {
                if (player.grasps[index] == null) {
                    hands_are_full = false;
                    continue;
                }

                if (player.grasps[index].grabbed is Spear) {
                    spear_in_hand = true;
                    break;
                }
            }

            if (hands_are_full || spear_in_hand) {
                // this case makes sure that you can pick up spears directly to your back;
                spear_on_back.abstractStick = null;
                spear_on_back.spear = null;
            } else {
                spear_on_back.abstractStick = abstract_on_back_sticks[current_spear_index];
                spear_on_back.spear = (Spear)spear_on_back.abstractStick.Spear.realizedObject;
            }
        }

        if (spear_on_back.increment) {
            ++spear_on_back.counter;

            // prioritize SpearToHand; otherwise you need to drop spears in order to dual 
            // wield with Spearmaster;
            if (spear_on_back.counter > 20 && current_spear_index > -1 && main_hand_is_empty && !off_hand_holds_big_item) {
                spear_on_back.SpearToHand(eu);
                spear_on_back.counter = 0;
            }

            // check if you can SpearToBack
            if (spear_on_back.counter > 20 && current_spear_index < attached_fields.max_spear_count - 1) {
                foreach (Creature.Grasp? grasp in player.grasps) {
                    if (grasp?.grabbed is not Spear spear) continue;
                    player.bodyChunks[0].pos += Custom.DirVec(grasp.grabbed.firstChunk.pos, player.bodyChunks[0].pos) * 2f;
                    spear_on_back.SpearToBack(spear);
                    spear_on_back.counter = 0;
                    break;
                }
            }

            // hands are empty // check if you can SpearToHand
            if (spear_on_back.counter > 20 && current_spear_index > -1) {
                spear_on_back.SpearToHand(eu);
                spear_on_back.counter = 0;
            }
        } else {
            spear_on_back.counter = 0;
        }

        if (!player.input[0].pckp) {
            spear_on_back.interactionLocked = false;
        }
        spear_on_back.increment = false;
    }
}
