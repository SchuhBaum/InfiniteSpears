using System.Collections.Generic;
using static Player;

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
        public readonly bool is_blacklisted;
        public readonly bool has_infinite_spears;
        public readonly int max_spear_count;

        // this is only used when max_spear_count > 1; this extends the number of 
        // backspears that you can carry; in this case the vanilla abstract_on_back 
        // stick is ignored and null;
        public readonly List<AbstractOnBackStick> abstract_on_back_sticks = new();

        public Attached_Fields(int max_spear_count) {
            if (max_spear_count == 0) {
                is_blacklisted = true;
                has_infinite_spears = false;
                this.max_spear_count = 0;
                return;
            }

            if (max_spear_count is < 0 or > 7) {
                is_blacklisted = false;
                has_infinite_spears = true;
                this.max_spear_count = 1;
                return;
            }

            is_blacklisted = false;
            has_infinite_spears = false;
            this.max_spear_count = max_spear_count;
        }
    }
}
