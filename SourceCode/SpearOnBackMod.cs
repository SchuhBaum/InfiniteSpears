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

    private static readonly float[] spearXYModifier = new float[7] { 0.0f, -0.4f, -0.2f, -0.2f, -0.6f, -0.6f, -0.8f };
    private static readonly float[] spearZModifier = new float[7] { 0.0f, 0.0f, 2f, -2f, 2f, -2f, 0.0f };

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

    public static bool DespawnSpear(SpearOnBack spear_on_back) {
        if (spear_on_back.interactionLocked) return false;
        if (spear_on_back.owner is not Player player) return false;
        if (!player.input[0].pckp) return false;

        if (spear_on_back.spear?.abstractPhysicalObject is not AbstractSpear carriedAbstractSpear) return false;
        if (player.grasps[0]?.grabbed?.abstractPhysicalObject is not AbstractSpear heldAbstractSpear) return false;

        if (carriedAbstractSpear.explosive != heldAbstractSpear.explosive) return false;
        if (carriedAbstractSpear.electric != heldAbstractSpear.electric) return false;
        if (carriedAbstractSpear.needle != heldAbstractSpear.needle) return false;
        if (carriedAbstractSpear.hue != heldAbstractSpear.hue) return false;

        ++spear_on_back.counter;
        if (spear_on_back.counter <= 20) {
            // keep the counter from resetting
            return true;
        }

        heldAbstractSpear.realizedObject.Destroy();
        heldAbstractSpear.Destroy();
        spear_on_back.counter = 0;
        spear_on_back.interactionLocked = true;
        return true;
    }

    public static void DropAllSpears(SpearOnBack spear_on_back) {
        if (spear_on_back.owner is not Player player) return;
        Attached_Fields attached_fields = player.Get_Attached_Fields();
        if (attached_fields.max_spear_count == -1) return;
        spear_on_back.spear = null;

        for (int stickIndex = attached_fields.abstract_on_back_sticks.Count - 1; stickIndex >= 0; --stickIndex) {
            AbstractOnBackStick abstract_on_back_stick = attached_fields.abstract_on_back_sticks[stickIndex];
            if (abstract_on_back_stick.Spear.realizedObject is Spear spear) {
                spear.firstChunk.vel = player.mainBodyChunk.vel + Custom.RNV() * 3f * Random.value;
                spear.ChangeMode(Weapon.Mode.Free);
            }
            abstract_on_back_stick.Deactivate(); // removes the backspear as well
        }
    }

    public static void SpawnSpear(SpearOnBack spear_on_back, in AbstractSpear? abstractSpear) {
        if (abstractSpear == null) return;
        if (spear_on_back.abstractStick != null) return;
        if (spear_on_back.spear != null) return;
        if (spear_on_back.owner is not Player player) return;

        AbstractPhysicalObject abstractPlayer = player.abstractPhysicalObject;
        World world = abstractSpear.world;

        if (world == null) return;

        // not sure what is random about spears;
        // but for consistency;
        EntityID id = world.game.GetNewID();
        id.altSeed = abstractSpear.ID.RandomSeed;

        AbstractSpear newAbstractSpear = new(world, null, abstractPlayer.pos, id, abstractSpear.explosive, abstractSpear.electric) {
            electricCharge = abstractSpear.electricCharge,
            hue = abstractSpear.hue,
            needle = abstractSpear.needle,
        };

        newAbstractSpear.RealizeInRoom();
        if (newAbstractSpear.realizedObject is not Spear newSpear) return;

        if (abstractSpear.realizedObject is Spear spear) {
            newSpear.spearmasterNeedle = spear.spearmasterNeedle;
            newSpear.spearmasterNeedleType = spear.spearmasterNeedleType;
            newSpear.spearmasterNeedle_hasConnection = spear.spearmasterNeedle_hasConnection;
            newSpear.spearmasterNeedle_fadecounter = spear.spearmasterNeedle_fadecounter;
        }

        newSpear.ChangeMode(Weapon.Mode.OnBack);
        spear_on_back.spear = newSpear;
        spear_on_back.abstractStick = new AbstractOnBackStick(abstractPlayer, newAbstractSpear);
        spear_on_back.interactionLocked = true;
        player.noPickUpOnRelease = 20;
        player.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, player.mainBodyChunk);
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
        if (attached_fields.max_spear_count == -1 || attached_fields.is_blacklisted) {
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

        if (attached_fields.max_spear_count == -1) {
            if (spear_on_back.spear?.slatedForDeletetion == true) {
                spear_on_back.spear = null; // prevents backspear from being dropped when spear gets abstracted
            }

            orig(spear_on_back, actually_viewed, eu);
            return;
        }

        List<AbstractOnBackStick> abstract_on_back_sticks = attached_fields.abstract_on_back_sticks;
        int currentSpearIndex = abstract_on_back_sticks.Count - 1;

        if (currentSpearIndex > -1) {
            Vector2 chunk0_pos = player.mainBodyChunk.pos;
            Vector2 chunk1_pos = player.bodyChunks[1].pos;

            if (player.graphicsModule is PlayerGraphics playerGraphics) {
                chunk0_pos = Vector2.Lerp(playerGraphics.drawPositions[0, 0], playerGraphics.head.pos, 0.2f);
                chunk1_pos = playerGraphics.drawPositions[1, 0];
            }

            Vector2 body_vector = Custom.DirVec(chunk1_pos, chunk0_pos);
            bool hasGravityAndStuff = player.Consious && player.bodyMode != BodyModeIndex.ZeroG && player.room.gravity > 0.0;

            if (hasGravityAndStuff) {
                spear_on_back.flip = player.bodyMode != BodyModeIndex.Default || player.animation != AnimationIndex.None || (!player.standing || player.bodyChunks[1].pos.y >= player.bodyChunks[0].pos.y - 6.0) ? (player.bodyMode != BodyModeIndex.Stand || player.input[0].x == 0 ? Custom.LerpAndTick(spear_on_back.flip, player.flipDirection * Mathf.Abs(body_vector.x), 0.15f, 1f / 6f) : Custom.LerpAndTick(spear_on_back.flip, player.input[0].x, 0.02f, 0.1f)) : Custom.LerpAndTick(spear_on_back.flip, player.input[0].x * 0.3f, 0.05f, 0.02f);
            } else {
                spear_on_back.flip = Custom.LerpAndTick(spear_on_back.flip, 0.0f, 0.15f, 1f / 7f);
            }

            for (int index = 0; index <= currentSpearIndex; ++index) {
                if (abstract_on_back_sticks[index].Spear.realizedObject is Spear spear && !spear.slatedForDeletetion) {
                    if (hasGravityAndStuff) {
                        if (spear_on_back.counter > 12 && !spear_on_back.interactionLocked && (player.input[0].x != 0 && player.standing)) {
                            float handDirection = 0.0f;
                            for (int index2 = 0; index2 < player.grasps.Length; ++index2) {
                                if (player.grasps[index2] == null) {
                                    handDirection = index2 != 0 ? 1f : -1f;
                                    break;
                                }
                            }
                            spear.setRotation = new Vector2?(Custom.DegToVec(Custom.AimFromOneVectorToAnother(chunk1_pos, chunk0_pos) + Custom.LerpMap(spear_on_back.counter, 12f, 20f, 0.0f, 360f * handDirection)));
                        } else {
                            // structure: [default] - PerpendicularVector * ([rotation when standing] + [rotation when walking/crawling])
                            spear.setRotation = new Vector2?(body_vector - Custom.PerpendicularVector(body_vector) * ((1.2f + spearXYModifier[index]) * (1f - Mathf.Abs(spear_on_back.flip)) - spear_on_back.flip * 0.02f * spearZModifier[index]));
                            spear.ChangeOverlap(body_vector.y < -0.1 && player.bodyMode != BodyModeIndex.ClimbingOnBeam);
                        }
                    } else {
                        // structure: [default] - PerpendicularVector * [rotation when standing]
                        spear.setRotation = new Vector2?(body_vector - Custom.PerpendicularVector(body_vector) * (1.2f + spearXYModifier[index]));
                        spear.ChangeOverlap(false);
                    }

                    // structure: [height] - PerpendicularVector * [depth] // flip is about zero when standing, i.e. no depth required
                    spear.firstChunk.MoveFromOutsideMyUpdate(eu, Vector2.Lerp(chunk1_pos, chunk0_pos, 0.5f - spearXYModifier[index] / 4f) - Custom.PerpendicularVector(chunk1_pos, chunk0_pos) * (7.5f + spearZModifier[index]) * spear_on_back.flip);
                    spear.firstChunk.vel = player.mainBodyChunk.vel;
                    spear.rotationSpeed = 0.0f;
                }
            }
        }
    }

    private static void SpearOnBack_SpearToBack(On.Player.SpearOnBack.orig_SpearToBack orig, SpearOnBack spear_on_back, Spear spear) {
        if (spear_on_back.owner is not Player player) {
            orig(spear_on_back, spear);
            return;
        }

        Attached_Fields attached_fields = player.Get_Attached_Fields();
        if (attached_fields.max_spear_count == -1 || attached_fields.is_blacklisted) {
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
        player.room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, player.mainBodyChunk);

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

        if (attached_fields.max_spear_count == -1) {
            if (spear_on_back.spear == null) return;
            if (spear_on_back.abstractStick == null) return;

            AbstractSpear abstractSpear = spear_on_back.spear.abstractSpear; // spear is not null
            orig(spear_on_back, eu);
            SpawnSpear(spear_on_back, abstractSpear);
            return;
        }

        List<AbstractOnBackStick> abstract_on_back_sticks = player.abstractCreature.Get_Attached_Fields().abstract_on_back_sticks;
        int currentSpearIndex = abstract_on_back_sticks.Count - 1;

        if (currentSpearIndex <= -1) return;
        if (abstract_on_back_sticks[currentSpearIndex] is not AbstractOnBackStick abstract_on_back_stick) return;
        if (abstract_on_back_stick.Spear.realizedObject is not Spear spear) return;

        foreach (Creature.Grasp? grasp in player.grasps) {
            if (grasp != null && player.Grabability(grasp.grabbed) >= ObjectGrabability.BigOneHand) return;
        }

        int graspUsed = -1;
        for (int index = 0; index < 2; ++index) {
            if (player.grasps[index] == null) {
                graspUsed = index;
                break;
            }
        }

        if (graspUsed == -1) return;

        if (player.graphicsModule is PlayerGraphics playerGraphics) {
            // spear_on_back.spear.firstChunk.MoveFromOutsideMyUpdate(eu, playerGraphics.hands[graspUsed].pos);
            spear.firstChunk.MoveFromOutsideMyUpdate(eu, playerGraphics.hands[graspUsed].pos);
        }

        spear.ChangeMode(Weapon.Mode.Free);
        player.SlugcatGrab(spear, graspUsed);
        spear_on_back.interactionLocked = true;
        player.noPickUpOnRelease = 20;

        player.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, player.mainBodyChunk);
        abstract_on_back_stick.Deactivate(); // removes as well

        // abstract_on_back_sticks.Remove(abstract_on_back_stick);
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

        if (attached_fields.max_spear_count == -1) {
            // consistency check
            if (spear_on_back.spear == null && spear_on_back.abstractStick != null) {
                spear_on_back.spear = (Spear)spear_on_back.abstractStick.Spear.realizedObject;
            }

            if (DespawnSpear(spear_on_back)) return;
            orig(spear_on_back, eu);
            return;
        }

        List<AbstractOnBackStick> abstract_on_back_sticks = attached_fields.abstract_on_back_sticks;
        int current_spear_index = abstract_on_back_sticks.Count - 1;

        if (current_spear_index == -1) {
            spear_on_back.spear = null;
        } else if (current_spear_index == attached_fields.max_spear_count - 1) {
            spear_on_back.spear = (Spear)abstract_on_back_sticks[current_spear_index].Spear.realizedObject;
        } else {
            bool spear_in_hand = false;
            bool hands_are_full = true;

            // carrying another spear and getting one off the back can be possible at the same time;
            // check player hands to decide which one is the case;
            for (int index = 0; index < 2; ++index) {
                if (player.grasps[index] == null) {
                    hands_are_full = false;
                } else if (player.grasps[index].grabbed is Spear) {
                    spear_in_hand = true;
                    break;
                }
            }

            if (hands_are_full || spear_in_hand) {
                spear_on_back.spear = null;
            } else {
                spear_on_back.spear = (Spear)abstract_on_back_sticks[current_spear_index].Spear.realizedObject;
            }
        }

        if (spear_on_back.increment) {
            ++spear_on_back.counter;
            // check if you can SpearToBack
            if (current_spear_index < attached_fields.max_spear_count - 1 && spear_on_back.counter > 20) {
                for (int index = 0; index < 2; ++index) {
                    if (player.grasps[index] != null && player.grasps[index].grabbed is Spear) {
                        player.bodyChunks[0].pos += Custom.DirVec(player.grasps[index].grabbed.firstChunk.pos, player.bodyChunks[0].pos) * 2f;
                        spear_on_back.SpearToBack(player.grasps[index].grabbed as Spear);
                        spear_on_back.counter = 0;
                        break;
                    }
                }
            }

            // hands are empty // check if you can SpearToHand
            if (current_spear_index > -1 && spear_on_back.counter > 20) {
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
