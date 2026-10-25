namespace PatronesAplicados.RealImplementation
{
    public interface IWeaponUpgrade
    {
        void Apply(ref WeaponRuntimeStats stats);
    }
}