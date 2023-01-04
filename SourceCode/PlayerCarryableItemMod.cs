namespace InfiniteSpears
{
    internal class PlayerCarryableItemMod
    {
        internal static void OnEnable()
        {
            On.PlayerCarryableItem.Update += PlayerCarryableItem_Update; // disable collision between backspears
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void PlayerCarryableItem_Update(On.PlayerCarryableItem.orig_Update orig, PlayerCarryableItem playerCarryableItem, bool eu)
        {
            orig(playerCarryableItem, eu);
            if (playerCarryableItem is Spear spear && spear.mode == Weapon.Mode.OnBack)
            {
                spear.firstChunk.collideWithObjects = false;
            }
        }
    }
}