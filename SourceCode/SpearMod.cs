using System.Collections.Generic;
using static AbstractPhysicalObject;
using static InfiniteSpears.AbstractPlayerMod;
using static Player;

namespace InfiniteSpears;

internal class SpearMod {
    internal static void OnEnable() {
        On.Spear.RecreateSticksFromAbstract += Spear_RecreateSticksFromAbstract;
    }

    //
    // private
    //

    private static void Spear_RecreateSticksFromAbstract(On.Spear.orig_RecreateSticksFromAbstract orig, Spear spear) {
        // vanilla calls this function only when sticksRespawned is false => redundant;
        if (spear.sticksRespawned) return;

        // in what instance did this matter?;
        foreach (AbstractObjectStick abstract_object_stick in spear.abstractPhysicalObject.stuckObjects) {
            if (abstract_object_stick is not AbstractOnBackStick abstract_on_back_stick) continue;
            if (abstract_on_back_stick.Spear != spear.abstractPhysicalObject) continue;

            // I change the mode for the spear in all cases where an AbstractOnBackStick
            // is found since this means that the connection is still active, i.e. 
            // AbstractOnBackStick.Deactivate() was not called yet;
            spear.ChangeMode(Weapon.Mode.OnBack);

            if (abstract_on_back_stick.Player.realizedObject is not Player player) continue;
            Attached_Fields attached_fields = player.Get_Attached_Fields();
            if (attached_fields.is_blacklisted) break;
            if (attached_fields.has_infinite_spears) break;

            // 
            // there was or is a bug where the index in _spear_xy_modifier[index] is 
            // out of range; since I already check if the index is smaller than zero
            // it must be too large; ther are only two instances where sticks are
            // added; the other instance checks if it would exceed the max spear count;
            // so it must be this instance => add checks to prevent that;
            // 

            List<AbstractOnBackStick> abstract_on_back_sticks = attached_fields.abstract_on_back_sticks;
            if (abstract_on_back_sticks.Count >= attached_fields.max_spear_count) break;
            if (abstract_on_back_sticks.Contains(abstract_on_back_stick)) break;
            abstract_on_back_sticks.Add(abstract_on_back_stick);
        }
        orig(spear);
    }
}
