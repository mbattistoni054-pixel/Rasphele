namespace PatronesAplicados
{
    /// <summary>
    /// Interfaz que debe implementar el Sujeto (el objeto que est siendo observado).
    /// Define mtodos para aadir, eliminar y notificar a los observadores.
    /// </summary>
    public interface IObservable
    {
        void Subscribe(IObserver observer);
        void Unsubscribe(IObserver observer);
        void NotifyObservers(string action);
    }
}
