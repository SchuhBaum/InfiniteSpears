
namespace InfiniteSpears;

public static class PlayerMod {
    //
    // parameters and variables
    //

    private static Hook? hook_Player_CanPutSlugToBack = null;
    private static Hook? hook_Player_CanPutSpearToBack = null;
    private static Hook? hook_Player_CanRetrieveSlugFromBack = null;
    private static Hook? hook_Player_CanRetrieveSpearFromBack = null;

    public static Attached_Fields Get_Attached_Fields(this Player player) => player.abstractCreature.Get_Attached_Fields();

    //
    // main
    //

    internal static void On_Config_Changed() {
        hook_Player_CanPutSlugToBack?.Dispose();
        hook_Player_CanPutSpearToBack?.Dispose();
        hook_Player_CanRetrieveSlugFromBack?.Dispose();
        hook_Player_CanRetrieveSpearFromBack?.Dispose();

        hook_Player_CanPutSlugToBack         = null;
        hook_Player_CanPutSpearToBack        = null;
        hook_Player_CanRetrieveSlugFromBack  = null;
        hook_Player_CanRetrieveSpearFromBack = null;

        On.Player.Regurgitate -= Player_Regurgitate;

        if (Option_SwallowedItems) {
            On.Player.Regurgitate += Player_Regurgitate;
        }

        if (Type.GetType("Player, Assembly-CSharp") is Type player_class) {
            if (Option_SlugsAndSpears) {
                try {
                    hook_Player_CanPutSlugToBack = new Hook(
                        player_class.GetProperty(
                            "CanPutSlugToBack",
                            BindingFlags.Public | BindingFlags.Instance
                        ).GetMethod,
                        typeof(PlayerMod).GetMethod("Player_CanPutSlugToBack_Allow")
                    );
                } catch (Exception exception) {
                    Debug.Log($"{mod_id}: {exception}");
                }

                try {
                    hook_Player_CanPutSpearToBack = new Hook(
                        player_class.GetProperty(
                            "CanPutSpearToBack",
                            BindingFlags.Public | BindingFlags.Instance
                        ).GetMethod,
                        typeof(PlayerMod).GetMethod("Player_CanPutSpearToBack")
                    );
                } catch (Exception exception) {
                    Debug.Log($"{mod_id}: {exception}");
                }

                try {
                    hook_Player_CanRetrieveSlugFromBack = new Hook(
                        player_class.GetProperty(
                            "CanRetrieveSlugFromBack",
                            BindingFlags.Public | BindingFlags.Instance
                        ).GetMethod,
                        typeof(PlayerMod).GetMethod("Player_CanRetrieveSlugFromBack")
                    );
                } catch (Exception exception) {
                    Debug.Log($"{mod_id}: {exception}");
                }

                try {
                    hook_Player_CanRetrieveSpearFromBack = new Hook(
                        player_class.GetProperty(
                            "CanRetrieveSpearFromBack",
                            BindingFlags.Public | BindingFlags.Instance
                        ).GetMethod,
                        typeof(PlayerMod).GetMethod("Player_CanRetrieveSpearFromBack")
                    );
                } catch (Exception exception) {
                    Debug.Log($"{mod_id}: {exception}");
                }
            } else {
                try {
                    hook_Player_CanPutSlugToBack = new Hook(
                        player_class.GetProperty(
                            "CanPutSlugToBack",
                            BindingFlags.Public | BindingFlags.Instance
                        ).GetMethod,
                        typeof(PlayerMod).GetMethod("Player_CanPutSlugToBack_Prevent")
                    );
                } catch (Exception exception) {
                    Debug.Log($"{mod_id}: {exception}");
                }
            }
        } else {
            Debug.Log($"{mod_id}: Failed to create property hooks for class Player.");
        }
    }

    internal static void OnEnable() {
        IL.Player.GrabUpdate += IL_Player_GrabUpdate;
        On.Player.ctor += Player_Ctor; // create list of backspears
        On.Player.Die += Player_Die; // drop all backspears
        On.Player.Stun += Player_Stun; // drop all backspears
    }

    //
    // public
    //

    public static bool Player_CanPutSlugToBack_Allow(Func<Player, bool> orig, Player player) { // Option_SlugsAndSpears
        // When using Slugpup Safari, this hook gets ignored.
        if (player.input[0].y == 0) return false;
        return (ModManager.MSC || ModManager.CoopAvailable) && player.slugOnBack != null && !player.slugOnBack.interactionLocked && player.slugOnBack.slugcat == null;
    }

    public static bool Player_CanPutSlugToBack_Prevent(Func<Player, bool> orig, Player player) {
        bool vanilla_result = orig(player);
        if (player.Get_Attached_Fields().abstract_on_back_sticks.Count > 0) return false;
        return vanilla_result;
    }

    public static bool Player_CanPutSpearToBack(Func<Player, bool> orig, Player player) { // Option_SlugsAndSpears
        if (player.input[0].y != 0) return false;
        return player.spearOnBack != null && !player.spearOnBack.interactionLocked && player.spearOnBack.spear == null;
    }

    public static bool Player_CanRetrieveSlugFromBack(Func<Player, bool> orig, Player player) { // Option_SlugsAndSpears
        bool result = orig(player);
        if (player.input[0].y == 0) return false;
        return result;
    }

    public static bool Player_CanRetrieveSpearFromBack(Func<Player, bool> orig, Player player) { // Option_SlugsAndSpears
        bool result = orig(player);
        if (player.input[0].y != 0) return false;
        return result;
    }

    public static int PlayerMod_UpdateFreeHand(int free_hand, Player player, AbstractSpear abstract_spear) {
        // Who knows if this is going to work. Rain Meadow does update this as
        // well. That is the reason why I want to return an int. Then it can be
        // chained. But the intention is that you don't execute the function
        // SlugcatGrab when I run my part. Now it is executed and -1 might not
        // propagate as expected.

        // vanilla case
        if (free_hand > -1) {
            return free_hand;
        }

        Attached_Fields attached_fields = player.Get_Attached_Fields();

        // I might not want to check this since you can have a backspear perk as well
        // if (attached_fields.isBlacklisted) return -1;

        if (player.spearOnBack is not SpearOnBack spear_on_back) return -1;
        if (spear_on_back.abstractStick != null) return -1;
        if (spear_on_back.spear != null) return -1;

        spear_on_back.abstractStick = new AbstractOnBackStick(player.abstractPhysicalObject, abstract_spear);
        spear_on_back.spear = (Spear)spear_on_back.abstractStick.Spear.realizedObject; // null is okay;
        spear_on_back.interactionLocked = true;
        player.noPickUpOnRelease = 20;

        if (abstract_spear.realizedObject is not Spear spear) return -1;
        spear.ChangeMode(Weapon.Mode.OnBack);
        return -1;
    }

    public static bool Uses_A_Persistant_Tracker(AbstractPhysicalObject abstract_physical_object) {
        //
        // copy & paste vanilla function but it works with "key item tracking" disabled
        // as well;
        //

        AbstractObjectType type = abstract_physical_object.type;
        if (type == AbstractObjectType.NSHSwarmer) return true;
        if (type == MoreSlugcatsEnums.AbstractObjectType.EnergyCell) {
            return true;
        }
        if (type == MoreSlugcatsEnums.AbstractObjectType.JokeRifle) {
            return true;
        }

        if (
            abstract_physical_object is DataPearl.AbstractDataPearl pearl
            && DataPearl.PearlIsNotMisc(pearl.dataPearlType)
        ) return true;
        if (
            abstract_physical_object is VultureMask.AbstractVultureMask mask
            && mask.scavKing
        ) return true;

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
                Debug.Log($"{mod_id}: IL_Player_GrabUpdate: Index {cursor.Index}"); // 597
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
                Debug.Log($"{mod_id}: IL_Player_GrabUpdate failed.");
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
                Debug.Log($"{mod_id}: IL_Player_GrabUpdate: Index {cursor.Index}");
            }

            // Diffuse the if condition but jumping in place. We want to be
            // compatible to Rain Meadow here as much as possible. Do NOT remove
            // these instructions.
            cursor.GotoNext(instruction => instruction.MatchBle(out ILLabel _));
            cursor.Goto(cursor.Index + 1);
            cursor.MarkLabel((ILLabel)cursor.Prev.Operand);

            cursor.GotoNext(MoveType.After,
                    instruction => instruction.MatchCall<Player>(nameof(Player.FreeHand)));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc, 19);
            cursor.EmitDelegate(PlayerMod_UpdateFreeHand);
        } else {
            if (can_log_il_hooks) {
                Debug.Log($"{mod_id}: IL_Player_GrabUpdate failed.");
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

        Attached_Fields attached_fields = new Attached_Fields(max_spear_count: 0);
        if (player.SlugCatClass is not Name slugcat_name) {
            _all_attached_fields.Add(abstract_player, attached_fields);
            return;
        }

        // General
        if (slugcat_name == Yellow) {
            attached_fields = new Attached_Fields(Option_Max_Spear_Count_Yellow);
        } else if (slugcat_name == White) {
            attached_fields = new Attached_Fields(Option_Max_Spear_Count_White);
        } else if (slugcat_name == Red) {
            attached_fields = new Attached_Fields(Option_Max_Spear_Count_Red);

        // MSC DLC
        } else if (ModManager.MSC && slugcat_name == Gourmand) {
            attached_fields = new Attached_Fields(Option_Max_Spear_Count_Gourmand);
        } else if (ModManager.MSC && slugcat_name == Artificer) {
            attached_fields = new Attached_Fields(Option_Max_Spear_Count_Artificer);
        } else if (ModManager.MSC && slugcat_name == Rivulet) {
            attached_fields = new Attached_Fields(Option_Max_Spear_Count_Rivulet);
        } else if (ModManager.MSC && slugcat_name == MoreSlugcatsEnums.SlugcatStatsName.Spear) {
            attached_fields = new Attached_Fields(Option_Max_Spear_Count_Spearmaster);
        } else if (ModManager.MSC && slugcat_name == Saint) {
            attached_fields = new Attached_Fields(Option_Max_Spear_Count_Saint);

        // Watcher DLC
        } else if (ModManager.Watcher && slugcat_name == Watcher.WatcherEnums.SlugcatStatsName.Watcher) {
            attached_fields = new Attached_Fields(Option_Max_Spear_Count_Watcher);

        // Custom
        } else if (ModManager.MSC && slugcat_name == Sofanthiel) {
            attached_fields = new Attached_Fields(Option_Max_Spear_Count_Sofanthiel);
        } else {
            var parsed_name = Regex.Replace(slugcat_name.value, @"[^a-zA-Z0-9_]", "_");
            if (!blacklisted_custom_slugcat_names.Contains(parsed_name)) {
                attached_fields = new Attached_Fields(Option_Max_Spear_Count_Custom_Slugcat(parsed_name));
            }
        }
        _all_attached_fields.Add(abstract_player, attached_fields);

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

    // Option_SwallowedItems
    private static void Player_Regurgitate(
        On.Player.orig_Regurgitate orig, Player player
    ) {
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

        // Pebble pearls are not tracked but are special nonetheless. They
        // track all other pebble pearls such that they can form an orbit. I
        // don't want to deal with that. Currently the game freezes when you
        // try to regurgitate the "copied" normal pearl.
        AbstractObjectType type = abstract_physical_object.type;
        if (type == AbstractObjectType.PebblesPearl) {
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

        if (
            type == AbstractObjectType.Lantern ||
            type == AbstractObjectType.Rock ||
            type == AbstractObjectType.ScavengerBomb ||
            type == DLCSharedEnums.AbstractObjectType.SingularityBomb ||
            type == WatcherEnums.AbstractObjectType.Boomerang
        ) {
            player.objectInStomach = new(abstract_physical_object.world, type, null, abstract_physical_object.pos, id);
            return;
        }

        if (
            type == AbstractObjectType.FirecrackerPlant ||
            type == AbstractObjectType.FlareBomb || 
            type == AbstractObjectType.FlyLure ||
            type == AbstractObjectType.JellyFish ||
            type == AbstractObjectType.KarmaFlower ||
            type == AbstractObjectType.Mushroom ||
            type == AbstractObjectType.NeedleEgg ||
            type == AbstractObjectType.PuffBall ||
            type == AbstractObjectType.SlimeMold ||
            type == DLCSharedEnums.AbstractObjectType.DandelionPeach ||
            type == DLCSharedEnums.AbstractObjectType.GlowWeed ||
            type == DLCSharedEnums.AbstractObjectType.GooieDuck
        ) {
            player.objectInStomach = new AbstractConsumable(abstract_physical_object.world, type, null, abstract_physical_object.pos, id, -1, -1, null);
            return;
        }

        switch (abstract_physical_object) {
            case AbstractBullet abstract_bullet:
                player.objectInStomach = new AbstractBullet(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_bullet.bulletType, abstract_bullet.timeToLive);
                return;

            case AbstractCreature abstract_creature:
                player.objectInStomach = new AbstractCreature(abstract_physical_object.world, abstract_creature.creatureTemplate, null, abstract_physical_object.pos, id);
                return;

            case AbstractSpear abstract_spear:
                player.objectInStomach = new AbstractSpear(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_spear.explosive, abstract_spear.electric) {
                    electricCharge = abstract_spear.electricCharge,
                    hue = abstract_spear.hue,
                    needle = abstract_spear.needle,
                };
                return;

            case BubbleGrass.AbstractBubbleGrass abstract_bubble_grass:
                player.objectInStomach = new BubbleGrass.AbstractBubbleGrass(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_bubble_grass.oxygenLeft, -1, -1, null);
                return;

            case DangleFruit.AbstractDangleFruit abstract_dangle_fruit:
                player.objectInStomach = new DangleFruit.AbstractDangleFruit(abstract_physical_object.world, null, abstract_physical_object.pos, id, -1, -1, abstract_dangle_fruit.rotted, null);
                return;

            case DataPearl.AbstractDataPearl abstract_data_pearl:
                player.objectInStomach = new DataPearl.AbstractDataPearl(abstract_physical_object.world, type, null, abstract_physical_object.pos, id, -1, -1, null, abstract_data_pearl.dataPearlType);
                return;

            case EggBugEgg.AbstractBugEgg abstract_egg_bug_egg:
                player.objectInStomach = new EggBugEgg.AbstractBugEgg(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_egg_bug_egg.hue);
                return;

            case FireEgg.AbstractBugEgg abstract_fire_egg:
                player.objectInStomach = new FireEgg.AbstractBugEgg(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_fire_egg.hue);
                return;

            case GraffitiBomb.AbstractGraffitiBomb abstract_graffiti_bomb:
                player.objectInStomach = new GraffitiBomb.AbstractGraffitiBomb(abstract_physical_object.world, null, abstract_physical_object.pos, id, -1, -1, null, abstract_graffiti_bomb.color);
                return;

            case LillyPuck.AbstractLillyPuck abstract_lilly_puck:
                player.objectInStomach = new LillyPuck.AbstractLillyPuck(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_lilly_puck.bites, -1, -1, null);
                return;

            case OverseerCarcass.AbstractOverseerCarcass abstract_overseer_carcass:
                player.objectInStomach = new OverseerCarcass.AbstractOverseerCarcass(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_overseer_carcass.color, abstract_overseer_carcass.ownerIterator);
                return;

            case SeedCob.AbstractSeedCob abstract_seed_cob:
                player.objectInStomach = new SeedCob.AbstractSeedCob(abstract_physical_object.world, null, abstract_physical_object.pos, id, -1, -1, abstract_seed_cob.dead, null);
                return;

            case SporePlant.AbstractSporePlant abstract_spore_plant:
                player.objectInStomach = new SporePlant.AbstractSporePlant(abstract_physical_object.world, null, abstract_physical_object.pos, id, -1, -1, null, abstract_spore_plant.used, abstract_spore_plant.pacified);
                return;

            case VultureMask.AbstractVultureMask abstract_vulture_mask:
                player.objectInStomach = new VultureMask.AbstractVultureMask(abstract_physical_object.world, null, abstract_physical_object.pos, id, abstract_vulture_mask.colorSeed, abstract_vulture_mask.king);
                return;

            case Watcher.BoxWorm.Larva.AbstractLarva abstract_larva:
                player.objectInStomach = new Watcher.BoxWorm.Larva.AbstractLarva(abstract_physical_object.world, null, abstract_physical_object.pos, id);
                return;

            case WaterNut.AbstractWaterNut abstract_water_nut:
                player.objectInStomach = new WaterNut.AbstractWaterNut(abstract_physical_object.world, null, abstract_physical_object.pos, id, -1, -1, null, abstract_water_nut.swollen);
                return;

            default:
                Debug.Log($"{mod_id}.Player_Regurgitate: [WARNING] Trying to duplicate {type}. But it is not whitelisted.");
                return;
        }
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
