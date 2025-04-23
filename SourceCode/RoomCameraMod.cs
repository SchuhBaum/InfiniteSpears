using System.Collections.Generic;
using UnityEngine;

using static Player;
using static AbstractPhysicalObject;

namespace InfiniteSpears;

internal class RoomCameraMod {
    internal static void OnEnable() {
        On.RoomCamera.WarpMoveCameraActual += RoomCamera_WarpMoveCameraActual;
    }

    //
    // private
    //

    private static void RoomCamera_WarpMoveCameraActual(On.RoomCamera.orig_WarpMoveCameraActual orig, RoomCamera room_camera, Room loading_room, int cam_pos_index) {
        // There is a bug where the player gets spawned twice when carrying a
        // backspear. Drop them as a workaround.
        foreach (AbstractCreature abstract_player in loading_room.world.game.Players) {
            if (abstract_player.realizedCreature is Player player && player.spearOnBack is SpearOnBack spear_on_back) {
                SpearOnBackMod.DropAllSpears(spear_on_back);
            }
        }
        orig(room_camera, loading_room, cam_pos_index);
    }
}
