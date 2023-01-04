namespace InfiniteSpears
{
    internal class SpearMod
    {
        internal static void OnEnable()
        {
            On.Spear.RecreateSticksFromAbstract += Spear_RecreateSticksFromAbstract;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void Spear_RecreateSticksFromAbstract(On.Spear.orig_RecreateSticksFromAbstract orig, Spear spear)
        {
            if (spear.sticksRespawned)
            {
                return;
            }

            foreach (AbstractPhysicalObject.AbstractObjectStick abstractObjectStick in spear.abstractPhysicalObject.stuckObjects)
            {
                if (abstractObjectStick is Player.AbstractOnBackStick abstractOnBackStick && abstractOnBackStick.Player.realizedObject is Player player && player.spearOnBack != null)
                {
                    spear.ChangeMode(Weapon.Mode.OnBack);
                    SpearOnBackMod.abstractOnBackSticks[player.playerState.playerNumber].Add(abstractOnBackStick);
                }
            }
            orig(spear);
        }
    }
}