namespace InfiniteSpears;

internal class PlayerCarryableItemMod {
    internal static void OnEnable() {
        On.PlayerCarryableItem.Update += PlayerCarryableItem_Update; // disable collision between backspears
    }

    // ----------------- //
    // private functions //
    // ----------------- //

    private static void PlayerCarryableItem_Update(On.PlayerCarryableItem.orig_Update orig, PlayerCarryableItem player_carryable_item, bool eu) {
        orig(player_carryable_item, eu);
        if (player_carryable_item is Spear spear && spear.mode == Weapon.Mode.OnBack) {
            spear.firstChunk.collideWithObjects = false;
        }
    }
}
