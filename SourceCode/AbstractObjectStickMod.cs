using UnityEngine;

namespace InfiniteSpears;

internal class AbstractObjectStickMod {
    internal static void OnEnable() {
        On.AbstractPhysicalObject.AbstractObjectStick.Deactivate += AbstractObjectStick_Deactivate;
    }

    //
    // private
    //

    private static void AbstractObjectStick_Deactivate(On.AbstractPhysicalObject.AbstractObjectStick.orig_Deactivate orig, AbstractPhysicalObject.AbstractObjectStick abstractObjectStick) {
        orig(abstractObjectStick);

        if (abstractObjectStick is not Player.AbstractOnBackStick abstractOnBackStick) return;
        if (abstractOnBackStick.Player is not AbstractCreature abstractPlayer) return;

        AbstractPlayerMod.Attached_Fields attached_fields = abstractPlayer.Get_Attached_Fields();
        if (attached_fields.is_blacklisted) return;

        // there seems to be sources where spear get deactivated but 
        // the mode is not changed and they clip through the floor;
        if (abstractOnBackStick.Spear.realizedObject is Spear spear && spear.mode == Weapon.Mode.OnBack) {
            Debug.Log("InfiniteSpears: Trying to deactivate AbstractOnBackStick but the spear is still on the back. Release spear.");
            spear.ChangeMode(Weapon.Mode.Free);
        }
        attached_fields.abstract_on_back_sticks.Remove(abstractOnBackStick);
    }
}
