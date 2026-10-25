using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados
{
    public interface IPausable
    {
        void OnPause();
        void OnResume();
    }

    public class PauseManager : MonoBehaviour
    {
        public static PauseManager Instance { get; private set; }

        private static readonly HashSet<IPausable> RegisteredPausables = new HashSet<IPausable>();
        private static readonly List<IPausable> IterationBuffer = new List<IPausable>(128);

        private static int _pauseStackCount;
        public static bool IsPaused { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                ResetPauseState();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening(GameEventKeys.PauseRequested, RequestPause);
                EventManager.Instance.StartListening(GameEventKeys.ResumeRequested, RequestResume);
            }
        }

        private void OnDisable()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening(GameEventKeys.PauseRequested, RequestPause);
                EventManager.Instance.StopListening(GameEventKeys.ResumeRequested, RequestResume);
            }
        }

        public static void Register(IPausable pausable)
        {
            if (pausable == null) return;

            if (RegisteredPausables.Add(pausable) && IsPaused)
            {
                pausable.OnPause();
            }
        }

        public static void Unregister(IPausable pausable)
        {
            if (pausable == null) return;
            RegisteredPausables.Remove(pausable);
        }

        public static void RequestPause()
        {
            _pauseStackCount++;
            if (_pauseStackCount == 1 && !IsPaused)
            {
                SetPausedInternal(true);
            }
        }

        public static void RequestResume()
        {
            _pauseStackCount--;
            if (_pauseStackCount <= 0)
            {
                _pauseStackCount = 0;
                if (IsPaused)
                {
                    SetPausedInternal(false);
                }
            }
        }

        public static void ForcePause()
        {
            if (_pauseStackCount <= 0) _pauseStackCount = 1;
            if (!IsPaused)
            {
                SetPausedInternal(true);
            }
        }

        public static void ResetPauseState()
        {
            _pauseStackCount = 0;
            if (IsPaused)
            {
                SetPausedInternal(false);
            }
        }

        public static void ClearAll()
        {
            _pauseStackCount = 0;
            IsPaused = false;
            RegisteredPausables.Clear();
            IterationBuffer.Clear();
        }

        private static void SetPausedInternal(bool paused)
        {
            IsPaused = paused;

            IterationBuffer.Clear();
            foreach (IPausable item in RegisteredPausables)
            {
                if (item != null)
                {
                    IterationBuffer.Add(item);
                }
            }

            for (int i = 0; i < IterationBuffer.Count; i++)
            {
                IPausable pausable = IterationBuffer[i];
                if (pausable == null) continue;

                if (paused) pausable.OnPause();
                else pausable.OnResume();
            }
        }
    }
}