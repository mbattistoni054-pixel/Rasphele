using System;
using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        private readonly Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }


        public void StartListening(string eventName, Action listener) => AddListener(eventName, listener);
        public void StartListening<T>(string eventName, Action<T> listener) => AddListener(eventName, listener);
        public void StartListening<T1, T2>(string eventName, Action<T1, T2> listener) => AddListener(eventName, listener);
        public void StartListening<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener) => AddListener(eventName, listener);

        private void AddListener(string eventName, Delegate listener)
        {
            if (string.IsNullOrEmpty(eventName) || listener == null) return;

            if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent) && thisEvent != null)
            {
                if (thisEvent.GetType() != listener.GetType())
                {
                    Debug.LogError($"[EventManager] Conflicto de tipos en el evento '{eventName}'. Registrado: {thisEvent.GetType().Name}, Intentado: {listener.GetType().Name}");
                    return;
                }
                eventDictionary[eventName] = Delegate.Combine(thisEvent, listener);
            }
            else
            {
                eventDictionary[eventName] = listener;
            }
        }


        public void StopListening(string eventName, Action listener) => RemoveListener(eventName, listener);
        public void StopListening<T>(string eventName, Action<T> listener) => RemoveListener(eventName, listener);
        public void StopListening<T1, T2>(string eventName, Action<T1, T2> listener) => RemoveListener(eventName, listener);
        public void StopListening<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener) => RemoveListener(eventName, listener);

        private void RemoveListener(string eventName, Delegate listener)
        {
            if (Instance == null || string.IsNullOrEmpty(eventName) || listener == null) return;

            if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent) && thisEvent != null)
            {
                if (thisEvent.GetType() != listener.GetType()) return;

                Delegate currentDel = Delegate.Remove(thisEvent, listener);
                if (currentDel == null)
                {
                    eventDictionary.Remove(eventName);
                }
                else
                {
                    eventDictionary[eventName] = currentDel;
                }
            }
        }


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