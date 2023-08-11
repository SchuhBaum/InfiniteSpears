using UnityEngine;
using static Weapon.Mode;

namespace InfiniteSpears;

internal class AbstractObjectStickMod {
    internal static void OnEnable() {
        On.AbstractPhysicalObject.AbstractObjectStick.Deactivate += AbstractObjectStick_Deactivate;
    }

    //
    // private
    //

    private static void AbstractObjectStick_Deactivate(On.AbstractPhysicalObject.AbstractObjectStick.orig_Deactivate orig, AbstractPhysicalObject.AbstractObjectStick abstract_object_stick) {
        orig(abstract_object_stick);

        if (abstract_object_stick is not Player.AbstractOnBackStick abstract_on_back_stick) return;
        if (abstract_on_back_stick.Player is not AbstractCreature abstract_player) return;

        // there seems to be sources where the spear gets deactivated but the mode is 
        // not changed and they clip through the floor;
        if (abstract_on_back_stick.Spear.realizedObject is Spear spear && spear.mode == OnBack) {
            Debug.Log("InfiniteSpears: Trying to deactivate AbstractOnBackStick but the spear is still on the back. Release spear.");
            spear.ChangeMode(Free);
        }

        // blacklisted means that max_spear_count is zero or one; if it is -1 there is
        // nothing to remove as well; in any case, I can simply call Remove(...) since 
        // it does not trigger an exception; it only returns false;
        // if (abstract_player.Get_Attached_Fields().is_blacklisted) return;
        abstract_player.Get_Attached_Fields().abstract_on_back_sticks.Remove(abstract_on_back_stick);
    }
}
