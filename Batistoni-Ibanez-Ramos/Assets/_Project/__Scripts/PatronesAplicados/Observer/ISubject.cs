namespace PatronesAplicados
{
    /// <summary>
    /// Interfaz que debe implementar el Sujeto (el objeto que est siendo observado).
    /// Define mtodos para aadir, eliminar y notificar a los observadores.
    /// </summary>
    public interface ISubject
    {
        void RegisterObserver(IObserver observer);
        void RemoveObserver(IObserver observer);
        void NotifyObservers();
    }
}
