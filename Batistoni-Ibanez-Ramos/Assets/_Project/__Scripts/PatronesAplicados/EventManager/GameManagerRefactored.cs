using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatronesAplicados
{
    public class GameManagerRefactored : MonoBehaviour
    {
        public static GameManagerRefactored Instance;

        [Header("Reloj del Nivel (Tiempo Jugado)")]
        private float timer = 0f;
        private int currentTimerSeconds = -1; // Variable de Optimizacin de UI
        private bool isGameActive = true;

        [Header("UI Mens de Fin de Nivel")]
        public GameObject gameOverCanvas;
        public GameObject levelCompleteCanvas;

        [Header("Nombres de Escenas")]
        public string mainMenuSceneName = "MainMenu";

        private int pauseCount;

        public GameObject globalVolume;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
            if (levelCompleteCanvas != null) levelCompleteCanvas.SetActive(false);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening("PlayerDeath", ShowGameOver);
                EventManager.Instance.StartListening("LevelComplete", ShowLevelComplete);
                EventManager.Instance.StartListening("PauseRequested", OnPauseRequested);
                EventManager.Instance.StartListening("ResumeRequested", OnResumeRequested);
            }
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening("PlayerDeath", ShowGameOver);
                EventManager.Instance.StopListening("LevelComplete", ShowLevelComplete);
                EventManager.Instance.StopListening("PauseRequested", OnPauseRequested);
                EventManager.Instance.StopListening("ResumeRequested", OnResumeRequested);
            }
        }

        private void Update()
        {
            if (!isGameActive) return;

            timer += Time.deltaTime;

            // OPTIMIZACIN Y DESACOPLAMIENTO
            // 1. Evitamos el acoplamiento directo a HUDManager.Instance (SOLID - Inversin de Dependencia).
            // 2. Solo disparamos el evento si el SEGUNDO entero cambi. Actualizar un TextMeshPro
            //    en cada frame causa severos picos de "Canvas Rebuild" y destruye los FPS en mviles/PC lentas.
            int newSeconds = Mathf.FloorToInt(timer);
            if (newSeconds > currentTimerSeconds)
            {
                currentTimerSeconds = newSeconds;
                
                if (EventManager.Instance != null)
                {
                    // Usamos la nueva sobrecarga genrica para enviar el dato numrico
                    EventManager.Instance.TriggerEvent("TimeUpdated", currentTimerSeconds);
                }
            }
        }

        private void OnPauseRequested()
        {
            if (pauseCount == 0) Time.timeScale = 0f;
            pauseCount++;
        }

        private void OnResumeRequested()
        {
            pauseCount--;
            if (pauseCount <= 0)
            {
                pauseCount = 0;
                Time.timeScale = 1f;
                if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
            }
        }

        public void ShowGameOver()
        {
            isGameActive = false;
            if (gameOverCanvas != null) gameOverCanvas.SetActive(true);

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowLevelComplete()
        {
            isGameActive = false;
            if (levelCompleteCanvas != null) levelCompleteCanvas.SetActive(true);

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
