namespace InfiniteSpears;

internal class WeaponMod {
    internal static void OnEnable() {
        On.Weapon.AddToContainer += Weapon_AddToContainer;
    }

    //
    // private
    //

    private static void Weapon_AddToContainer(On.Weapon.orig_AddToContainer orig, Weapon weapon, RoomCamera.SpriteLeaser sprite_leaser, RoomCamera room_camera, FContainer new_container) {
        if (new_container != null) {
            orig(weapon, sprite_leaser, room_camera, new_container);
            return;
        }

        weapon.inFrontOfObjects = 1; // load previous value instead of resetting
        orig(weapon, sprite_leaser, room_camera, new_container);
    }
}
