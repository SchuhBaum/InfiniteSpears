using System.Collections.Generic;

namespace InfiniteSpears
{
    internal static class AbstractPlayerMod
    {
        //
        // variables
        //

        internal static readonly Dictionary<AbstractCreature, AttachedFields> allAttachedFields = new();
        public static AttachedFields GetAttachedFields(this AbstractCreature abstractPlayer) => allAttachedFields[abstractPlayer];

        //
        //
        //

        public sealed class AttachedFields
        {
            public bool isBlacklisted = true;
            public List<Player.AbstractOnBackStick> abstractOnBackSticks = new(); // change to actual size when game is created
        }
    }
}