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
            if(abstractObjectStick is Player.AbstractOnBackStick abstractOnBackStick && abstractOnBackStick.Player is AbstractCreature abstractCreature && abstractOnBackStick.Spear is AbstractSpear) // JollyCoop uses Spear to store players
            {
                SpearOnBackMod.abstractOnBackSticks[((PlayerState)abstractCreature.state).playerNumber].Remove(abstractOnBackStick);
            }
        }
    }
}