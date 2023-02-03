using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace InfiniteSpears
{
    public class SpearOnBackMod
    {
        //
        // parameters
        //

        private static readonly float[] spearXYModifier = new float[7] { 0.0f, -0.4f, -0.2f, -0.2f, -0.6f, -0.6f, -0.8f };
        private static readonly float[] spearZModifier = new float[7] { 0.0f, 0.0f, 2f, -2f, 2f, -2f, 0.0f };

        //
        //
        //

        internal static void OnEnable()
        {
            On.Player.SpearOnBack.DropSpear += SpearOnBack_DropSpear;
            On.Player.SpearOnBack.GraphicsModuleUpdated += SpearOnBack_GraphicsModuleUpdated;
            On.Player.SpearOnBack.SpearToBack += SpearOnBack_SpearToBack;
            On.Player.SpearOnBack.SpearToHand += SpearOnBack_SpearToHand;
            On.Player.SpearOnBack.Update += SpearOnBack_Update;
        }

        //
        // public
        //

        public static bool DespawnSpear(Player.SpearOnBack spearOnBack)
        {
            if (spearOnBack.interactionLocked) return false;
            if (spearOnBack.owner is not Player player) return false;
            if (!player.input[0].pckp) return false;

            if (spearOnBack.spear?.abstractPhysicalObject is not AbstractSpear carriedAbstractSpear) return false;
            if (player.grasps[0]?.grabbed?.abstractPhysicalObject is not AbstractSpear heldAbstractSpear) return false;

            if (carriedAbstractSpear.explosive != heldAbstractSpear.explosive) return false;
            if (carriedAbstractSpear.electric != heldAbstractSpear.electric) return false;
            if (carriedAbstractSpear.needle != heldAbstractSpear.needle) return false;
            if (carriedAbstractSpear.hue != heldAbstractSpear.hue) return false;

            ++spearOnBack.counter;
            if (spearOnBack.counter <= 20)
            {
                // keep the counter from resetting
                return true;
            }

            heldAbstractSpear.realizedObject.Destroy();
            heldAbstractSpear.Destroy();
            spearOnBack.counter = 0;
            spearOnBack.interactionLocked = true;
            return true;
        }

        public static void DropAllSpears(Player.SpearOnBack spearOnBack)
        {
            if (MainMod.Option_MaxSpearCount == 1) return;
            if (spearOnBack.owner is not Player player) return;

            spearOnBack.spear = null;
            AbstractPlayerMod.AttachedFields attachedFields = player.abstractCreature.GetAttachedFields();

            for (int stickIndex = attachedFields.abstractOnBackSticks.Count - 1; stickIndex >= 0; --stickIndex)
            {
                Player.AbstractOnBackStick abstractOnBackStick = attachedFields.abstractOnBackSticks[stickIndex];
                if (abstractOnBackStick.Spear.realizedObject is Spear spear)
                {
                    spear.firstChunk.vel = player.mainBodyChunk.vel + Custom.RNV() * 3f * UnityEngine.Random.value;
                    spear.ChangeMode(Weapon.Mode.Free);
                }
                abstractOnBackStick.Deactivate(); // removes the backspear as well
            }
        }

        public static void SpawnSpearOnBack(Player.SpearOnBack spearOnBack, in AbstractSpear? abstractSpear)
        {
            if (abstractSpear == null) return;
            if (spearOnBack.spear != null) return;
            if (spearOnBack.abstractStick != null) return;
            if (spearOnBack.owner is not Player player) return;

            AbstractPhysicalObject abstractPlayer = player.abstractPhysicalObject;
            World world = abstractSpear.world;

            if (world == null) return;
            AbstractSpear newAbstractSpear = new(world, null, abstractPlayer.pos, world.game.GetNewID(), abstractSpear.explosive, abstractSpear.electric)
            {
                electricCharge = abstractSpear.electricCharge,
                hue = abstractSpear.hue,
                needle = abstractSpear.needle,
            };

            newAbstractSpear.RealizeInRoom();
            if (newAbstractSpear.realizedObject is not Spear newSpear) return;
            newSpear.ChangeMode(Weapon.Mode.OnBack);

            if (abstractSpear.realizedObject is Spear spear)
            {
                newSpear.spearmasterNeedle = spear.spearmasterNeedle;
                newSpear.spearmasterNeedleType = spear.spearmasterNeedleType;
                newSpear.spearmasterNeedle_hasConnection = spear.spearmasterNeedle_hasConnection;
                newSpear.spearmasterNeedle_fadecounter = spear.spearmasterNeedle_fadecounter;
            }

            spearOnBack.spear = newSpear;
            spearOnBack.abstractStick = new Player.AbstractOnBackStick(abstractPlayer, newAbstractSpear);
            spearOnBack.interactionLocked = true;
            player.noPickUpOnRelease = 20;
            player.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, player.mainBodyChunk);
        }

        //
        // private
        //

        private static void SpearOnBack_DropSpear(On.Player.SpearOnBack.orig_DropSpear orig, Player.SpearOnBack spearOnBack)
        {
            if (MainMod.Option_MaxSpearCount == 1 || spearOnBack.owner.GetAttachedFields().isBlacklisted)
            {
                orig(spearOnBack);
                return;
            }

            if (spearOnBack.interactionLocked) return;
            if (spearOnBack.owner is not Player player) return;

            List<Player.AbstractOnBackStick> abstractOnBackSticks = player.abstractCreature.GetAttachedFields().abstractOnBackSticks;
            int currentSpearIndex = abstractOnBackSticks.Count - 1;

            if (currentSpearIndex > -1 && abstractOnBackSticks[currentSpearIndex] is Player.AbstractOnBackStick abstractOnBackStick)
            {
                if (abstractOnBackStick.Spear.realizedObject is Spear spear)
                {
                    spear.firstChunk.vel = spearOnBack.owner.mainBodyChunk.vel + Custom.RNV() * 3f * UnityEngine.Random.value;
                    spear.ChangeMode(Weapon.Mode.Free);
                }
                abstractOnBackStick.Deactivate(); // removes now from abstractOnBackSticks as well
            }

            spearOnBack.spear = null;
            spearOnBack.interactionLocked = true;
        }

        private static void SpearOnBack_GraphicsModuleUpdated(On.Player.SpearOnBack.orig_GraphicsModuleUpdated orig, Player.SpearOnBack spearOnBack, bool actuallyViewed, bool eu)
        {
            if (spearOnBack.owner.GetAttachedFields().isBlacklisted)
            {
                orig(spearOnBack, actuallyViewed, eu);
                return;
            }

            if (MainMod.Option_MaxSpearCount == 1)
            {
                if (spearOnBack.spear?.slatedForDeletetion == true)
                {
                    spearOnBack.spear = null; // prevents backspear from being dropped when spear gets abstracted
                }

                orig(spearOnBack, actuallyViewed, eu);
                return;
            }

            if (spearOnBack.owner is not Player player) return;

            List<Player.AbstractOnBackStick> abstractOnBackSticks = player.abstractCreature.GetAttachedFields().abstractOnBackSticks;
            int currentSpearIndex = abstractOnBackSticks.Count - 1;

            if (currentSpearIndex > -1)
            {
                Vector2 chunk0_pos = player.mainBodyChunk.pos;
                Vector2 chunk1_pos = player.bodyChunks[1].pos;

                if (player.graphicsModule is PlayerGraphics playerGraphics)
                {
                    chunk0_pos = Vector2.Lerp(playerGraphics.drawPositions[0, 0], playerGraphics.head.pos, 0.2f);
                    chunk1_pos = playerGraphics.drawPositions[1, 0];
                }

                Vector2 body_vector = Custom.DirVec(chunk1_pos, chunk0_pos);
                bool hasGravityAndStuff = player.Consious && player.bodyMode != Player.BodyModeIndex.ZeroG && player.room.gravity > 0.0;

                if (hasGravityAndStuff)
                {
                    spearOnBack.flip = player.bodyMode != Player.BodyModeIndex.Default || player.animation != Player.AnimationIndex.None || (!player.standing || player.bodyChunks[1].pos.y >= player.bodyChunks[0].pos.y - 6.0) ? (player.bodyMode != Player.BodyModeIndex.Stand || player.input[0].x == 0 ? Custom.LerpAndTick(spearOnBack.flip, player.flipDirection * Mathf.Abs(body_vector.x), 0.15f, 1f / 6f) : Custom.LerpAndTick(spearOnBack.flip, player.input[0].x, 0.02f, 0.1f)) : Custom.LerpAndTick(spearOnBack.flip, player.input[0].x * 0.3f, 0.05f, 0.02f);
                }
                else
                {
                    spearOnBack.flip = Custom.LerpAndTick(spearOnBack.flip, 0.0f, 0.15f, 1f / 7f);
                }

                for (int index = 0; index <= currentSpearIndex; ++index)
                {
                    if (abstractOnBackSticks[index].Spear.realizedObject is Spear spear && !spear.slatedForDeletetion)
                    {
                        if (hasGravityAndStuff)
                        {
                            if (spearOnBack.counter > 12 && !spearOnBack.interactionLocked && (player.input[0].x != 0 && player.standing))
                            {
                                float handDirection = 0.0f;
                                for (int index2 = 0; index2 < player.grasps.Length; ++index2)
                                {
                                    if (player.grasps[index2] == null)
                                    {
                                        handDirection = index2 != 0 ? 1f : -1f;
                                        break;
                                    }
                                }
                                spear.setRotation = new Vector2?(Custom.DegToVec(Custom.AimFromOneVectorToAnother(chunk1_pos, chunk0_pos) + Custom.LerpMap(spearOnBack.counter, 12f, 20f, 0.0f, 360f * handDirection)));
                            }
                            else
                            {
                                // structure: [default] - PerpendicularVector * ([rotation when standing] + [rotation when walking/crawling])
                                spear.setRotation = new Vector2?(body_vector - Custom.PerpendicularVector(body_vector) * ((1.2f + spearXYModifier[index]) * (1f - Mathf.Abs(spearOnBack.flip)) - spearOnBack.flip * 0.02f * spearZModifier[index]));
                                spear.ChangeOverlap(body_vector.y < -0.1 && player.bodyMode != Player.BodyModeIndex.ClimbingOnBeam);
                            }
                        }
                        else
                        {
                            // structure: [default] - PerpendicularVector * [rotation when standing]
                            spear.setRotation = new Vector2?(body_vector - Custom.PerpendicularVector(body_vector) * (1.2f + spearXYModifier[index]));
                            spear.ChangeOverlap(false);
                        }

                        // structure: [height] - PerpendicularVector * [depth] // flip is about zero when standing, i.e. no depth required
                        spear.firstChunk.MoveFromOutsideMyUpdate(eu, Vector2.Lerp(chunk1_pos, chunk0_pos, 0.5f - spearXYModifier[index] / 4f) - Custom.PerpendicularVector(chunk1_pos, chunk0_pos) * (7.5f + spearZModifier[index]) * spearOnBack.flip);
                        spear.firstChunk.vel = player.mainBodyChunk.vel;
                        spear.rotationSpeed = 0.0f;
                    }
                }
            }
        }

        private static void SpearOnBack_SpearToBack(On.Player.SpearOnBack.orig_SpearToBack orig, Player.SpearOnBack spearOnBack, Spear spear)
        {
            if (MainMod.Option_MaxSpearCount == 1 || spearOnBack.owner.GetAttachedFields().isBlacklisted)
            {
                orig(spearOnBack, spear);
                return;
            }

            if (spearOnBack.owner is not Player player) return;

            List<Player.AbstractOnBackStick> abstractOnBackSticks = player.abstractCreature.GetAttachedFields().abstractOnBackSticks;
            int currentSpearIndex = abstractOnBackSticks.Count - 1;

            if (currentSpearIndex < MainMod.Option_MaxSpearCount - 1)
            {
                for (int grasp = 0; grasp < 2; ++grasp)
                {
                    if (player.grasps[grasp] != null && player.grasps[grasp].grabbed == spear)
                    {
                        player.ReleaseGrasp(grasp);
                        break;
                    }
                }

                spear.ChangeMode(Weapon.Mode.OnBack);
                spearOnBack.interactionLocked = true;
                player.noPickUpOnRelease = 20;
                player.room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, player.mainBodyChunk);

                abstractOnBackSticks.Add(new Player.AbstractOnBackStick(player.abstractPhysicalObject, spear.abstractPhysicalObject));
            }
        }

        private static void SpearOnBack_SpearToHand(On.Player.SpearOnBack.orig_SpearToHand orig, Player.SpearOnBack spearOnBack, bool eu)
        {
            if (spearOnBack.owner.GetAttachedFields().isBlacklisted)
            {
                orig(spearOnBack, eu);
                return;
            }

            if (MainMod.Option_MaxSpearCount == 1)
            {
                if (spearOnBack.spear == null) return;
                if (spearOnBack.abstractStick == null) return;

                AbstractSpear abstractSpear = spearOnBack.spear.abstractSpear; // spear is not null
                orig(spearOnBack, eu);
                SpawnSpearOnBack(spearOnBack, abstractSpear);
            }
            else
            {
                if (spearOnBack.owner is not Player player) return;

                List<Player.AbstractOnBackStick> abstractOnBackSticks = player.abstractCreature.GetAttachedFields().abstractOnBackSticks;
                int currentSpearIndex = abstractOnBackSticks.Count - 1;

                if (currentSpearIndex > -1 && abstractOnBackSticks[currentSpearIndex] is Player.AbstractOnBackStick abstractOnBackStick && abstractOnBackStick.Spear.realizedObject is Spear spear)
                {
                    foreach (Creature.Grasp? grasp in player.grasps)
                    {
                        if (grasp != null && player.Grabability(grasp.grabbed) >= Player.ObjectGrabability.BigOneHand) return;
                    }

                    int graspUsed = -1;
                    for (int index = 0; index < 2 && graspUsed == -1; ++index)
                    {
                        if (player.grasps[index] == null)
                        {
                            graspUsed = index;
                        }
                    }

                    if (graspUsed == -1) return;

                    if (player.graphicsModule is PlayerGraphics playerGraphics)
                    {
                        spearOnBack.spear.firstChunk.MoveFromOutsideMyUpdate(eu, playerGraphics.hands[graspUsed].pos);
                    }

                    spear.ChangeMode(Weapon.Mode.Free);
                    player.SlugcatGrab(spear, graspUsed);
                    spearOnBack.interactionLocked = true;
                    player.noPickUpOnRelease = 20;

                    player.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, player.mainBodyChunk);
                    abstractOnBackStick.Deactivate(); // removes as well
                    //abstractOnBackSticks_.Remove(abstractOnBackStick);
                }
            }
        }

        private static void SpearOnBack_Update(On.Player.SpearOnBack.orig_Update orig, Player.SpearOnBack spearOnBack, bool eu)
        {
            if (spearOnBack.owner.GetAttachedFields().isBlacklisted)
            {
                orig(spearOnBack, eu);
                return;
            }

            if (MainMod.Option_MaxSpearCount == 1)
            {
                if (spearOnBack.spear == null && spearOnBack.abstractStick != null) // consistency check
                {
                    spearOnBack.spear = (Spear)spearOnBack.abstractStick.Spear.realizedObject;
                }

                if (DespawnSpear(spearOnBack)) return;
                orig(spearOnBack, eu);
                return;
            }

            if (spearOnBack.owner is not Player player) return;

            List<Player.AbstractOnBackStick> abstractOnBackSticks = player.abstractCreature.GetAttachedFields().abstractOnBackSticks;
            int currentSpearIndex = abstractOnBackSticks.Count - 1;

            if (currentSpearIndex == -1)
            {
                if (spearOnBack.spear != null)
                {
                    spearOnBack.spear = null;
                }
            }
            else if (currentSpearIndex == MainMod.Option_MaxSpearCount - 1)
            {
                spearOnBack.spear = (Spear)abstractOnBackSticks[currentSpearIndex].Spear.realizedObject;
            }
            else
            {
                bool spearInHand = false;
                bool handsAreFull = true;

                // carrying another spear and getting one off the back can be possible at the same time // check player hands to decide which one is the case
                for (int index = 0; index < 2; ++index)
                {
                    if (player.grasps[index] == null)
                    {
                        handsAreFull = false;
                    }
                    else if (player.grasps[index].grabbed is Spear)
                    {
                        spearInHand = true;
                        break;
                    }
                }

                if (handsAreFull || spearInHand)
                {
                    spearOnBack.spear = null;
                }
                else
                {
                    spearOnBack.spear = (Spear)abstractOnBackSticks[currentSpearIndex].Spear.realizedObject;
                }
            }

            if (spearOnBack.increment)
            {
                ++spearOnBack.counter;
                // check if you can SpearToBack
                if (currentSpearIndex < MainMod.Option_MaxSpearCount - 1 && spearOnBack.counter > 20)
                {
                    for (int index = 0; index < 2; ++index)
                    {
                        if (player.grasps[index] != null && player.grasps[index].grabbed is Spear)
                        {
                            player.bodyChunks[0].pos += Custom.DirVec(player.grasps[index].grabbed.firstChunk.pos, player.bodyChunks[0].pos) * 2f;
                            spearOnBack.SpearToBack(player.grasps[index].grabbed as Spear);
                            spearOnBack.counter = 0;
                            break;
                        }
                    }
                }

                // hands are empty // check if you can SpearToHand
                if (currentSpearIndex > -1 && spearOnBack.counter > 20)
                {
                    spearOnBack.SpearToHand(eu);
                    spearOnBack.counter = 0;
                }
            }
            else
            {
                spearOnBack.counter = 0;
            }

            if (!player.input[0].pckp)
            {
                spearOnBack.interactionLocked = false;
            }
            spearOnBack.increment = false;
        }
    }
}