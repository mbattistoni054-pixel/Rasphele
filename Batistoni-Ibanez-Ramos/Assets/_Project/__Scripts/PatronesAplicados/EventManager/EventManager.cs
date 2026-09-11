using System;
using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados
{
    /// <summary>
    /// EventManager: Implementa el patrn Singleton y un Bus de Eventos (Event Bus).
    /// Sirve para centralizar la comunicacin entre scripts, reduciendo el acoplamiento (dependencias directas).
    /// Cumple con el Principio de Inversin de Dependencias (SOLID) al permitir que los mdulos dependan de abstracciones (eventos) y no de implementaciones concretas.
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        // Cambiamos 'Action' por 'Delegate' para soportar eventos con y sin parmetros genricos (Action<T>)
        private Dictionary<string, Delegate> eventDictionary;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                eventDictionary = new Dictionary<string, Delegate>();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // --- Suscripcin ---

        public void StartListening(string eventName, Action listener)
        {
            AddListener(eventName, listener);
        }

        public void StartListening<T>(string eventName, Action<T> listener)
        {
            AddListener(eventName, listener);
        }

        public void StartListening<T1, T2>(string eventName, Action<T1, T2> listener)
        {
            AddListener(eventName, listener);
        }

        public void StartListening<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener)
        {
            AddListener(eventName, listener);
        }

        private void AddListener(string eventName, Delegate listener)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            {
                eventDictionary[eventName] = Delegate.Combine(thisEvent, listener);
            }
            else
            {
                eventDictionary.Add(eventName, listener);
            }
        }

        // --- Desuscripcin ---

        public void StopListening(string eventName, Action listener)
        {
            RemoveListener(eventName, listener);
        }

        public void StopListening<T>(string eventName, Action<T> listener)
        {
            RemoveListener(eventName, listener);
        }

        public void StopListening<T1, T2>(string eventName, Action<T1, T2> listener)
        {
            RemoveListener(eventName, listener);
        }

        public void StopListening<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener)
        {
            RemoveListener(eventName, listener);
        }

        private void RemoveListener(string eventName, Delegate listener)
        {
            if (Instance == null) return;

            if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            {
                eventDictionary[eventName] = Delegate.Remove(thisEvent, listener);
            }
        }

        // --- Disparo de Eventos ---

        public void TriggerEvent(string eventName)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            {
                (thisEvent as Action)?.Invoke();
            }
        }

        public void TriggerEvent<T>(string eventName, T parameter)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            {
                (thisEvent as Action<T>)?.Invoke(parameter);
            }
        }

        public void TriggerEvent<T1, T2>(string eventName, T1 param1, T2 param2)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            {
                (thisEvent as Action<T1, T2>)?.Invoke(param1, param2);
            }
        }

        public void TriggerEvent<T1, T2, T3>(string eventName, T1 param1, T2 param2, T3 param3)
        {
            if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            {
                (thisEvent as Action<T1, T2, T3>)?.Invoke(param1, param2, param3);
            }
        }
    }
}
