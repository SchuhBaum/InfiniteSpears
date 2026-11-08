
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
            Debug.Log($"{mod_id}: Trying to deactivate AbstractOnBackStick but the spear {spear.abstractPhysicalObject.ID} is still on the back. Release spear.");
            spear.ChangeMode(Free);
        }

        var player = abstract_player.realizedCreature as Player;
        if (player == null) return;

        var attached_fields = player.Get_Attached_Fields();
        if (attached_fields.is_blacklisted) return;
        if (attached_fields.has_infinite_spears) return;

        attached_fields.abstract_on_back_sticks.Remove(abstract_on_back_stick);
    }
}
