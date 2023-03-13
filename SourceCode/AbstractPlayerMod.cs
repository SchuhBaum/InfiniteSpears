using System.Collections.Generic;

namespace InfiniteSpears;

public static class AbstractPlayerMod
{
    //
    // variables
    //

    internal static readonly Dictionary<AbstractCreature, Attached_Fields> all_attached_fields = new();
    public static Attached_Fields Get_Attached_Fields(this AbstractCreature abstractPlayer) => all_attached_fields[abstractPlayer];

    //
    //
    //

    public sealed class Attached_Fields
    {
        public bool is_blacklisted = true;
        public List<Player.AbstractOnBackStick> abstract_on_back_sticks = new(); // change to actual size when game is created
    }
}