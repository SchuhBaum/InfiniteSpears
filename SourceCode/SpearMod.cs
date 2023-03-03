namespace InfiniteSpears;

internal class SpearMod
{
    internal static void OnEnable()
    {
        On.Spear.RecreateSticksFromAbstract += Spear_RecreateSticksFromAbstract;
    }

    //
    // private
    //

    private static void Spear_RecreateSticksFromAbstract(On.Spear.orig_RecreateSticksFromAbstract orig, Spear spear)
    {
        if (spear.sticksRespawned) return;

        foreach (AbstractPhysicalObject.AbstractObjectStick abstractObjectStick in spear.abstractPhysicalObject.stuckObjects)
        {
            if (abstractObjectStick is Player.AbstractOnBackStick abstractOnBackStick && abstractOnBackStick.Player.realizedObject is Player player && player.spearOnBack != null)
            {
                AbstractPlayerMod.AttachedFields attachedFields = player.abstractCreature.GetAttachedFields();
                if (attachedFields.isBlacklisted) break;

                attachedFields.abstractOnBackSticks.Add(abstractOnBackStick);
                spear.ChangeMode(Weapon.Mode.OnBack);
            }
        }
        orig(spear);
    }
}