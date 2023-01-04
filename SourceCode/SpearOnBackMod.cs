using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace InfiniteSpears
{
    public class SpearOnBackMod
    {
        public static List<Player.AbstractOnBackStick>[] abstractOnBackSticks = new List<Player.AbstractOnBackStick>[4]; // change to actual size when game is created
        public static int maxSpearCount = 1;

        private static readonly float[] spearXYModifier = new float[7] { 0.0f, -0.4f, -0.2f, -0.2f, -0.6f, -0.6f, -0.8f };
        private static readonly float[] spearZModifier = new float[7] { 0.0f, 0.0f, 2f, -2f, 2f, -2f, 0.0f };

        internal static void OnEnable()
        {
            On.Player.SpearOnBack.DropSpear += SpearOnBack_DropSpear;
            On.Player.SpearOnBack.GraphicsModuleUpdated += SpearOnBack_GraphicsModuleUpdated;
            On.Player.SpearOnBack.SpearToBack += SpearOnBack_SpearToBack;
            On.Player.SpearOnBack.SpearToHand += SpearOnBack_SpearToHand;
            On.Player.SpearOnBack.Update += SpearOnBack_Update;
        }

        // ---------------- //
        // public functions //
        // ---------------- //

        public static void DropAllSpears(Player.SpearOnBack spearOnBack)
        {
            if (maxSpearCount == 1 || spearOnBack.owner == null)
            {
                return;
            }

            spearOnBack.spear = null;
            int playerNumber = spearOnBack.owner.playerState.playerNumber;

            foreach (Player.AbstractOnBackStick abstractOnBackStick in new List<Player.AbstractOnBackStick>(abstractOnBackSticks[playerNumber]))
            {
                if (abstractOnBackStick.Spear.realizedObject is Spear spear)
                {
                    spear.firstChunk.vel = spearOnBack.owner.mainBodyChunk.vel + Custom.RNV() * 3f * UnityEngine.Random.value;
                    spear.ChangeMode(Weapon.Mode.Free);
                }
                abstractOnBackStick.Deactivate(); // removes the backspear as well
            }
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void DespawnSpear(Player.SpearOnBack spearOnBack, Spear spear)
        {
            if (spearOnBack.owner == null || spearOnBack.spear == null || spear == null)
            {
                return;
            }

            if (spearOnBack.abstractStick != null)
            {
                spearOnBack.abstractStick.Deactivate();
                spearOnBack.abstractStick = null;
            }

            spearOnBack.spear.Destroy();
            spearOnBack.spear = null;
            spearOnBack.SpearToBack(spear);
        }

        private static void SpawnSpearOnBack(Player.SpearOnBack spearOnBack, bool explosive, int? charge = null)
        {
            if (spearOnBack.owner == null || spearOnBack.spear != null || spearOnBack.abstractStick != null)
            {
                return;
            }

            AbstractPhysicalObject abstractPlayer = spearOnBack.owner.abstractPhysicalObject;
            World world = abstractPlayer.world;
            AbstractSpear? abstractSpear;

            if (charge == null)
            {
                abstractSpear = new AbstractSpear(world, null, abstractPlayer.pos, world.game.GetNewID(), explosive);
            }
            else
            {
                abstractSpear = ElectricSpearMod.Ctor(world, abstractPlayer.pos, world.game.GetNewID(), (int)charge);
            }

            if (abstractSpear == null)
            {
                return;
            }

            abstractSpear.RealizeInRoom();
            spearOnBack.spear = (Spear)abstractSpear.realizedObject;
            spearOnBack.spear.ChangeMode(Weapon.Mode.OnBack);
            spearOnBack.abstractStick = new Player.AbstractOnBackStick(abstractPlayer, abstractSpear);

            spearOnBack.interactionLocked = true;
            spearOnBack.owner.noPickUpOnRelease = 20;
            spearOnBack.owner.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, spearOnBack.owner.mainBodyChunk);
        }


        private static void SpearOnBack_DropSpear(On.Player.SpearOnBack.orig_DropSpear orig, Player.SpearOnBack spearOnBack)
        {
            if (maxSpearCount == 1)
            {
                orig(spearOnBack);
                return;
            }

            if (spearOnBack.owner == null || spearOnBack.interactionLocked)
            {
                return;
            }

            int playerNumber = spearOnBack.owner.playerState.playerNumber;
            List<Player.AbstractOnBackStick> abstractOnBackSticks_ = abstractOnBackSticks[playerNumber];
            int currentSpearIndex = abstractOnBackSticks_.Count - 1;

            if (currentSpearIndex > -1 && abstractOnBackSticks_[currentSpearIndex] is Player.AbstractOnBackStick abstractOnBackStick)
            {
                if (abstractOnBackStick.Spear.realizedObject is Spear spear)
                {
                    spear.firstChunk.vel = spearOnBack.owner.mainBodyChunk.vel + Custom.RNV() * 3f * UnityEngine.Random.value;
                    spear.ChangeMode(Weapon.Mode.Free);
                }
                abstractOnBackStick.Deactivate(); // removes now from abstractOnBackSticks_ as well
            }

            spearOnBack.spear = null;
            spearOnBack.interactionLocked = true;
            // abstractOnBackSticks_.Remove(abstractOnBackSticks_[currentSpearIndex]);
        }

        private static void SpearOnBack_GraphicsModuleUpdated(On.Player.SpearOnBack.orig_GraphicsModuleUpdated orig, Player.SpearOnBack spearOnBack, bool actuallyViewed, bool eu)
        {
            if (maxSpearCount == 1)
            {
                if (spearOnBack.spear?.slatedForDeletetion == true)
                {
                    spearOnBack.spear = null; // prevents backspear from being dropped when spear gets abstracted
                }

                orig(spearOnBack, actuallyViewed, eu);
                return;
            }

            if (spearOnBack.owner == null)
            {
                return;
            }

            int playerNumber = spearOnBack.owner.playerState.playerNumber;
            List<Player.AbstractOnBackStick> abstractOnBackSticks_ = new(abstractOnBackSticks[playerNumber]);
            int currentSpearIndex = abstractOnBackSticks_.Count - 1;

            if (currentSpearIndex > -1)
            {
                Vector2 chunk0_pos = spearOnBack.owner.mainBodyChunk.pos;
                Vector2 chunk1_pos = spearOnBack.owner.bodyChunks[1].pos;

                if (spearOnBack.owner.graphicsModule is PlayerGraphics playerGraphics)
                {
                    chunk0_pos = Vector2.Lerp(playerGraphics.drawPositions[0, 0], playerGraphics.head.pos, 0.2f);
                    chunk1_pos = playerGraphics.drawPositions[1, 0];
                }

                Vector2 body_vector = Custom.DirVec(chunk1_pos, chunk0_pos);
                bool hasGravityAndStuff = spearOnBack.owner.Consious && spearOnBack.owner.bodyMode != Player.BodyModeIndex.ZeroG && spearOnBack.owner.room.gravity > 0.0;

                if (hasGravityAndStuff)
                {
                    spearOnBack.flip = spearOnBack.owner.bodyMode != Player.BodyModeIndex.Default || spearOnBack.owner.animation != Player.AnimationIndex.None || (!spearOnBack.owner.standing || spearOnBack.owner.bodyChunks[1].pos.y >= spearOnBack.owner.bodyChunks[0].pos.y - 6.0) ? (spearOnBack.owner.bodyMode != Player.BodyModeIndex.Stand || spearOnBack.owner.input[0].x == 0 ? Custom.LerpAndTick(spearOnBack.flip, spearOnBack.owner.flipDirection * Mathf.Abs(body_vector.x), 0.15f, 1f / 6f) : Custom.LerpAndTick(spearOnBack.flip, spearOnBack.owner.input[0].x, 0.02f, 0.1f)) : Custom.LerpAndTick(spearOnBack.flip, spearOnBack.owner.input[0].x * 0.3f, 0.05f, 0.02f);
                }
                else
                {
                    spearOnBack.flip = Custom.LerpAndTick(spearOnBack.flip, 0.0f, 0.15f, 1f / 7f);
                }

                for (int index = 0; index <= currentSpearIndex; ++index)
                {
                    if (abstractOnBackSticks_[index].Spear.realizedObject is Spear spear && !spear.slatedForDeletetion)
                    {
                        if (hasGravityAndStuff)
                        {
                            if (spearOnBack.counter > 12 && !spearOnBack.interactionLocked && (spearOnBack.owner.input[0].x != 0 && spearOnBack.owner.standing))
                            {
                                float handDirection = 0.0f;
                                for (int index2 = 0; index2 < spearOnBack.owner.grasps.Length; ++index2)
                                {
                                    if (spearOnBack.owner.grasps[index2] == null)
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
                                spear.ChangeOverlap(body_vector.y < -0.1 && spearOnBack.owner.bodyMode != Player.BodyModeIndex.ClimbingOnBeam);
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
                        spear.firstChunk.vel = spearOnBack.owner.mainBodyChunk.vel;
                        spear.rotationSpeed = 0.0f;
                    }
                }
            }
        }

        private static void SpearOnBack_SpearToBack(On.Player.SpearOnBack.orig_SpearToBack orig, Player.SpearOnBack spearOnBack, Spear spear)
        {
            if (maxSpearCount == 1)
            {
                orig(spearOnBack, spear);
                return;
            }

            if (spearOnBack.owner == null)
            {
                return;
            }

            int playerNumber = spearOnBack.owner.playerState.playerNumber;
            List<Player.AbstractOnBackStick> abstractOnBackSticks_ = abstractOnBackSticks[playerNumber];
            int currentSpearIndex = abstractOnBackSticks_.Count - 1;

            if (currentSpearIndex < maxSpearCount - 1)
            {
                for (int grasp = 0; grasp < 2; ++grasp)
                {
                    if (spearOnBack.owner.grasps[grasp] != null && spearOnBack.owner.grasps[grasp].grabbed == spear)
                    {
                        spearOnBack.owner.ReleaseGrasp(grasp);
                        break;
                    }
                }

                spear.ChangeMode(Weapon.Mode.OnBack);
                spearOnBack.interactionLocked = true;
                spearOnBack.owner.noPickUpOnRelease = 20;
                spearOnBack.owner.room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, spearOnBack.owner.mainBodyChunk);

                abstractOnBackSticks[playerNumber].Add(new Player.AbstractOnBackStick(spearOnBack.owner.abstractPhysicalObject, spear.abstractPhysicalObject));
            }
        }

        private static void SpearOnBack_SpearToHand(On.Player.SpearOnBack.orig_SpearToHand orig, Player.SpearOnBack spearOnBack, bool eu)
        {
            if (maxSpearCount == 1)
            {
                if (spearOnBack.spear == null || spearOnBack.abstractStick == null)
                {
                    return;
                }

                bool explosive = spearOnBack.spear.abstractSpear.explosive; // spear is not null
                int? charge = MainMod.IsElectricSpearModEnabled ? ElectricSpearMod.GetCharge(spearOnBack.spear.abstractSpear) : null;
                orig(spearOnBack, eu);
                SpawnSpearOnBack(spearOnBack, explosive, charge);
            }
            else
            {
                if (spearOnBack.owner == null)
                {
                    return;
                }

                int playerNumber = spearOnBack.owner.playerState.playerNumber;
                List<Player.AbstractOnBackStick> abstractOnBackSticks_ = abstractOnBackSticks[playerNumber];
                int currentSpearIndex = abstractOnBackSticks_.Count - 1;

                if (currentSpearIndex > -1 && abstractOnBackSticks_[currentSpearIndex] is Player.AbstractOnBackStick abstractOnBackStick && abstractOnBackStick.Spear.realizedObject is Spear spear)
                {
                    for (int index = 0; index < 2; ++index)
                    {
                        if (spearOnBack.owner.grasps[index] != null && spearOnBack.owner.Grabability(spearOnBack.owner.grasps[index].grabbed) >= Player.ObjectGrabability.BigOneHand)
                        {
                            return;
                        }
                    }

                    int graspUsed = -1;
                    for (int index = 0; index < 2 && graspUsed == -1; ++index)
                    {
                        if (spearOnBack.owner.grasps[index] == null)
                        {
                            graspUsed = index;
                        }
                    }

                    if (graspUsed == -1)
                    {
                        return;
                    }

                    if (spearOnBack.owner.graphicsModule is PlayerGraphics playerGraphics)
                    {
                        spearOnBack.spear.firstChunk.MoveFromOutsideMyUpdate(eu, playerGraphics.hands[graspUsed].pos);
                    }

                    spear.ChangeMode(Weapon.Mode.Free);
                    spearOnBack.owner.SlugcatGrab(spear, graspUsed);
                    spearOnBack.interactionLocked = true;
                    spearOnBack.owner.noPickUpOnRelease = 20;

                    spearOnBack.owner.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, spearOnBack.owner.mainBodyChunk);
                    abstractOnBackStick.Deactivate(); // removes as well
                    //abstractOnBackSticks_.Remove(abstractOnBackStick);
                }
            }
        }

        private static void SpearOnBack_Update(On.Player.SpearOnBack.orig_Update orig, Player.SpearOnBack spearOnBack, bool eu)
        {
            if (spearOnBack.owner == null)
            {
                return;
            }

            if (maxSpearCount == 1)
            {
                if (spearOnBack.spear == null && spearOnBack.abstractStick != null) // consistency check
                {
                    spearOnBack.spear = (Spear)spearOnBack.abstractStick.Spear.realizedObject;
                }

                if (!spearOnBack.interactionLocked && spearOnBack.owner.input[0].pckp && spearOnBack.spear is Spear carriedSpear && spearOnBack.owner.grasps[0] != null && spearOnBack.owner.grasps[0].grabbed is Spear heldSpear && carriedSpear.abstractSpear.explosive == heldSpear.abstractSpear.explosive) // despawn spear in right hand if same-type spears are carried on back and in right hand
                {
                    ++spearOnBack.counter;
                    if (spearOnBack.counter > 20)
                    {
                        DespawnSpear(spearOnBack, heldSpear);
                        spearOnBack.counter = 0;
                        spearOnBack.interactionLocked = true;
                    }
                    return;
                }

                orig(spearOnBack, eu);
                return;
            }

            int playerNumber = spearOnBack.owner.playerState.playerNumber;
            List<Player.AbstractOnBackStick> abstractOnBackSticks_ = abstractOnBackSticks[playerNumber];
            int currentSpearIndex = abstractOnBackSticks_.Count - 1;

            if (currentSpearIndex == -1)
            {
                if (spearOnBack.spear != null)
                {
                    spearOnBack.spear = null;
                }
            }
            else if (currentSpearIndex == maxSpearCount - 1)
            {
                spearOnBack.spear = (Spear)abstractOnBackSticks_[currentSpearIndex].Spear.realizedObject;
            }
            else
            {
                bool spearInHand = false;
                bool handsAreFull = true;

                // carrying another spear and getting one off the back can be possible at the same time // check player hands to decide which one is the case
                for (int index = 0; index < 2; ++index)
                {
                    if (spearOnBack.owner.grasps[index] == null)
                    {
                        handsAreFull = false;
                    }
                    else if (spearOnBack.owner.grasps[index].grabbed is Spear)
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
                    spearOnBack.spear = (Spear)abstractOnBackSticks_[currentSpearIndex].Spear.realizedObject;
                }
            }

            if (spearOnBack.increment)
            {
                ++spearOnBack.counter;
                // check if you can SpearToBack
                if (currentSpearIndex < maxSpearCount - 1 && spearOnBack.counter > 20)
                {
                    for (int index = 0; index < 2; ++index)
                    {
                        if (spearOnBack.owner.grasps[index] != null && spearOnBack.owner.grasps[index].grabbed is Spear)
                        {
                            spearOnBack.owner.bodyChunks[0].pos += Custom.DirVec(spearOnBack.owner.grasps[index].grabbed.firstChunk.pos, spearOnBack.owner.bodyChunks[0].pos) * 2f;
                            spearOnBack.SpearToBack(spearOnBack.owner.grasps[index].grabbed as Spear);
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

            if (!spearOnBack.owner.input[0].pckp)
            {
                spearOnBack.interactionLocked = false;
            }
            spearOnBack.increment = false;
        }
    }
}