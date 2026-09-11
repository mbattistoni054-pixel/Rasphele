namespace PatronesAplicados
{
    /// <summary>
    /// Interfaz que debe implementar cualquier clase que quiera observar un Subject.
    /// Define el mtodo de actualizacin que el Subject llamar.
    /// </summary>
    public interface IObserver
    {
        void OnNotify(float currentHealth, float maxHealth);
    }
}
