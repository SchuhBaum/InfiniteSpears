using System.Collections.Generic;

namespace InfiniteSpears;

public static class AbstractPlayerMod {
    //
    // variables
    //

    internal static readonly Dictionary<AbstractCreature, Attached_Fields> _all_attached_fields = new();
    public static Attached_Fields Get_Attached_Fields(this AbstractCreature abstract_player) => _all_attached_fields[abstract_player];

    //
    //
    //

    public sealed class Attached_Fields {
        public bool is_blacklisted = true;
        public int max_spear_count = 0;
        public List<Player.AbstractOnBackStick> abstract_on_back_sticks = new(); // change to actual size when game is created
    }
}
