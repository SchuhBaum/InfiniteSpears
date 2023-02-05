namespace InfiniteSpears
{
    internal class WeaponMod
    {
        internal static void OnEnable()
        {
            On.Weapon.AddToContainer += Weapon_AddToContainer;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void Weapon_AddToContainer(On.Weapon.orig_AddToContainer orig, Weapon weapon, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            if (newContainer != null)
            {
                orig(weapon, sLeaser, rCam, newContainer);
                return;
            }

            weapon.inFrontOfObjects = 1; // load previous value instead of resetting
            orig(weapon, sLeaser, rCam, newContainer);
        }
    }
}