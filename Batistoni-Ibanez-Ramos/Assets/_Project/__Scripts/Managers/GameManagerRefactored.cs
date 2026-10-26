using PatronesAplicados.RealImplementation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatronesAplicados
{
    public class GameManagerRefactored : MonoBehaviour
    {
        public static GameManagerRefactored Instance { get; private set; }

        private Transform _player;
        private bool _hasSearchedPlayer;

        public Transform Player
        {
            get
            {
                if (_player == null && !_hasSearchedPlayer)
                {
                    GameObject p = GameObject.FindGameObjectWithTag("Player");
                    if (p != null) _player = p.transform;
                    _hasSearchedPlayer = true;
                }
                return _player;
            }
        }

        [Header("Reloj del Nivel (Tiempo Jugado)")]
        private float _timer;
        private int _currentTimerSeconds = -1;
        private bool _isGameActive = true;

        [Header("UI Menús de Fin de Nivel")]
        public GameObject gameOverCanvas;
        public GameObject levelCompleteCanvas;

        [Header("Nombres de Escenas")]
        public string mainMenuSceneName = "MainMenu";

        public GameObject globalVolume;

        public void RegisterPlayer(Transform playerTransform)
        {
            _player = playerTransform;
            _hasSearchedPlayer = true;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DamageableRegistry.Clear();
                PauseManager.ResetPauseState();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
            if (levelCompleteCanvas != null) levelCompleteCanvas.SetActive(false);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening(GameEventKeys.PlayerDeath, ShowGameOver);
                EventManager.Instance.StartListening(GameEventKeys.LevelComplete, ShowLevelComplete);
                EventManager.Instance.StartListening(GameEventKeys.ResumeRequested, OnResumeRequested);
            }

            _ = Player;
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening(GameEventKeys.PlayerDeath, ShowGameOver);
                EventManager.Instance.StopListening(GameEventKeys.LevelComplete, ShowLevelComplete);
                EventManager.Instance.StopListening(GameEventKeys.ResumeRequested, OnResumeRequested);
            }
        }

        private void Update()
        {
            if (!_isGameActive || PauseManager.IsPaused) return;

            _timer += Time.deltaTime;

            int newSeconds = Mathf.FloorToInt(_timer);
            if (newSeconds > _currentTimerSeconds)
            {
                _currentTimerSeconds = newSeconds;

                if (EventManager.Instance != null)
                {
                    EventManager.Instance.TriggerEvent(GameEventKeys.TimeUpdated, _currentTimerSeconds);
                }
            }
        }

        private void OnResumeRequested()
        {
            if (!PauseManager.IsPaused && TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideTooltip();
            }
        }

        public void ShowGameOver() => EndGameSession(gameOverCanvas);

        public void ShowLevelComplete() => EndGameSession(levelCompleteCanvas);

        private void EndGameSession(GameObject targetCanvas)
        {
            _isGameActive = false;
            if (targetCanvas != null) targetCanvas.SetActive(true);

            PauseManager.ForcePause();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void RestartLevel()
        {
            ResetCaches();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            ResetCaches();
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void ResetCaches()
        {
            PauseManager.ClearAll();
            DamageableRegistry.Clear();
        }
    }
}