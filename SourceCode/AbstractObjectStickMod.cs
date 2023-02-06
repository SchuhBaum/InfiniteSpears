namespace InfiniteSpears
{
    internal class AbstractObjectStickMod
    {
        internal static void OnEnable()
        {
            On.AbstractPhysicalObject.AbstractObjectStick.Deactivate += AbstractObjectStick_Deactivate;
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void AbstractObjectStick_Deactivate(On.AbstractPhysicalObject.AbstractObjectStick.orig_Deactivate orig, AbstractPhysicalObject.AbstractObjectStick abstractObjectStick)
        {
            orig(abstractObjectStick);

            if (abstractObjectStick is not Player.AbstractOnBackStick abstractOnBackStick) return;
            if (abstractOnBackStick.Player is not AbstractCreature abstractPlayer) return;

            AbstractPlayerMod.AttachedFields attachedFields = abstractPlayer.GetAttachedFields();
            if (attachedFields.isBlacklisted) return;
            attachedFields.abstractOnBackSticks.Remove(abstractOnBackStick);
        }
    }
}