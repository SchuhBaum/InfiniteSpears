namespace InfiniteSpears;

internal class ShortcutHelperMod {
    internal static void OnEnable() {
        On.ShortcutHelper.AddPushOutObject += ShortcutHelper_AddPushOutObject; // prevent sound "spam" when carrying backSpears
    }

    // ----------------- //
    // private functions //
    // ----------------- //

    private static void ShortcutHelper_AddPushOutObject(On.ShortcutHelper.orig_AddPushOutObject orig, ShortcutHelper shortcut_helper, PhysicalObject physical_object) {
        if (physical_object.abstractPhysicalObject is AbstractSpear abstract_spear) {
            foreach (AbstractPhysicalObject abstract_physical_object in abstract_spear.GetAllConnectedObjects()) {
                if (abstract_physical_object is AbstractCreature) return;
            }
        }
        orig(shortcut_helper, physical_object);
    }
}
