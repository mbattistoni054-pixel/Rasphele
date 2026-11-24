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

        [Header("Estado en Tiempo Real (Solo Lectura)")]
        [SerializeField] private bool isPausedDebug;
        [SerializeField] private int pauseStackDebug;
        [SerializeField] private int registeredObjectsDebug;

        private static readonly HashSet<IPausable> RegisteredPausables = new HashSet<IPausable>();
        private static readonly List<IPausable> IterationBuffer = new List<IPausable>(128);

        private static int _pauseStackCount;
        private bool _isSubscribed;

        public static bool IsPaused { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                PruneDestroyedObjects();
                ResetPauseState();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            TrySubscribeEvents();
        }

        private void Start()
        {
            TrySubscribeEvents();
            SyncDebugInspector();
        }

        private void Update()
        {
            if (!_isSubscribed)
            {
                TrySubscribeEvents();
            }
        }

        private void OnDisable()
        {
            if (_isSubscribed && EventManager.Instance != null)
            {
                EventManager.Instance.StopListening(GameEventKeys.PauseRequested, RequestPause);
                EventManager.Instance.StopListening(GameEventKeys.ResumeRequested, RequestResume);
            }
            _isSubscribed = false;
        }

        private void TrySubscribeEvents()
        {
            if (_isSubscribed || EventManager.Instance == null) return;

            EventManager.Instance.StartListening(GameEventKeys.PauseRequested, RequestPause);
            EventManager.Instance.StartListening(GameEventKeys.ResumeRequested, RequestResume);
            _isSubscribed = true;
        }

        public static void Register(IPausable pausable)
        {
            if (pausable == null) return;

            if (RegisteredPausables.Add(pausable) && IsPaused)
            {
                pausable.OnPause();
            }

            if (Instance != null) Instance.SyncDebugInspector();
        }

        public static void Unregister(IPausable pausable)
        {
            if (pausable == null) return;
            RegisteredPausables.Remove(pausable);

            if (Instance != null) Instance.SyncDebugInspector();
        }

        public static void RequestPause()
        {
            _pauseStackCount++;
            if (!IsPaused)
            {
                SetPausedInternal(true);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Instance != null) Instance.SyncDebugInspector();
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

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (TooltipManager.Instance != null)
                {
                    TooltipManager.Instance.HideTooltip();
                }
            }

            if (Instance != null) Instance.SyncDebugInspector();
        }

        public static void ForcePause()
        {
            if (_pauseStackCount <= 0) _pauseStackCount = 1;
            if (!IsPaused)
            {
                SetPausedInternal(true);
            }

            if (Instance != null) Instance.SyncDebugInspector();
        }

        public static void ResetPauseState()
        {
            _pauseStackCount = 0;
            if (IsPaused)
            {
                SetPausedInternal(false);
            }

            if (Instance != null) Instance.SyncDebugInspector();
        }

        public static void ClearAll()
        {
            _pauseStackCount = 0;
            IsPaused = false;
            RegisteredPausables.Clear();
            IterationBuffer.Clear();

            if (Instance != null) Instance.SyncDebugInspector();
        }

        private static void PruneDestroyedObjects()
        {
            RegisteredPausables.RemoveWhere(item => item == null || (item is Object unityObj && unityObj == null));
        }

        private static void SetPausedInternal(bool paused)
        {
            IsPaused = paused;

            IterationBuffer.Clear();
            foreach (IPausable item in RegisteredPausables)
            {
                if (item != null && (!(item is Object unityObj) || unityObj != null))
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

        private void SyncDebugInspector()
        {
            isPausedDebug = IsPaused;
            pauseStackDebug = _pauseStackCount;
            registeredObjectsDebug = RegisteredPausables.Count;
        }
    }
}