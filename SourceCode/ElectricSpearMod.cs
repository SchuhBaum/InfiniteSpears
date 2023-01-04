namespace InfiniteSpears
{
    internal class ElectricSpearMod
    {
        internal static AbstractSpear? Ctor(World world, WorldCoordinate pos, EntityID ID, int charge) => new AbstractElectricSpear(world, null, pos, ID, charge);

        internal static int? GetCharge(AbstractSpear abstractSpear)
        {
            if (abstractSpear is AbstractElectricSpear abstractElectricSpear)
            {
                return abstractElectricSpear.charge;
            }
            return null;
        }
    }
}