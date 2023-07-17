using static InfiniteSpears.AbstractPlayerMod;

namespace InfiniteSpears;

internal class SpearMod {
    internal static void OnEnable() {
        On.Spear.RecreateSticksFromAbstract += Spear_RecreateSticksFromAbstract;
    }

    //
    // private
    //

    private static void Spear_RecreateSticksFromAbstract(On.Spear.orig_RecreateSticksFromAbstract orig, Spear spear) {
        if (spear.sticksRespawned) return;

        foreach (AbstractPhysicalObject.AbstractObjectStick abstract_object_stick in spear.abstractPhysicalObject.stuckObjects) {
            if (abstract_object_stick is Player.AbstractOnBackStick abstract_on_back_stick && abstract_on_back_stick.Player.realizedObject is Player player && player.spearOnBack != null) {
                Attached_Fields attached_fields = player.Get_Attached_Fields();
                if (attached_fields.is_blacklisted) break;

                attached_fields.abstract_on_back_sticks.Add(abstract_on_back_stick);
                spear.ChangeMode(Weapon.Mode.OnBack);
            }
        }
        orig(spear);
    }
}
