namespace InfiniteSpears
{
    internal class ShortcutHelperMod
    {
        internal static void OnEnable()
        {
            On.ShortcutHelper.AddPushOutObject += ShortcutHelper_AddPushOutObject; // prevent sound "spam" when carrying backSpears
        }

        // ----------------- //
        // private functions //
        // ----------------- //

        private static void ShortcutHelper_AddPushOutObject(On.ShortcutHelper.orig_AddPushOutObject orig, ShortcutHelper shortcutHelper, PhysicalObject physicalObject)
        {
            if (physicalObject.abstractPhysicalObject is AbstractSpear abstractSpear)
            {
                foreach (AbstractPhysicalObject abstractPhysicalObject in abstractSpear.GetAllConnectedObjects())
                {
                    if (abstractPhysicalObject is AbstractCreature)
                    {
                        return;
                    }
                }
            }
            orig(shortcutHelper, physicalObject);
        }
    }
}