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

        foreach (AbstractPhysicalObject.AbstractObjectStick abstractObjectStick in spear.abstractPhysicalObject.stuckObjects) {
            if (abstractObjectStick is Player.AbstractOnBackStick abstractOnBackStick && abstractOnBackStick.Player.realizedObject is Player player && player.spearOnBack != null) {
                AbstractPlayerMod.Attached_Fields attached_fields = player.abstractCreature.Get_Attached_Fields();
                if (attached_fields.is_blacklisted) break;

                attached_fields.abstract_on_back_sticks.Add(abstractOnBackStick);
                spear.ChangeMode(Weapon.Mode.OnBack);
            }
        }
        orig(spear);
    }
}
