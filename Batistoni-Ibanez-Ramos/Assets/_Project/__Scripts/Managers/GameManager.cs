using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Reloj del Nivel (Tiempo Jugado)")]
    private float timer = 0f;
    private bool isGameActive = true;

    public TextMeshProUGUI timeText;

    [Header("UI Menús de Fin de Nivel")]
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

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += ShowGameOver;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= ShowGameOver;
    }

    void Start()
    {
        // Asegurarnos de que los menús empiecen apagados
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        if (levelCompleteCanvas != null) levelCompleteCanvas.SetActive(false);
    }

    private void Update()
    {
        // Si el juego terminó (muerte o victoria), que el reloj se congele
        if (!isGameActive) return;

        // Sumamos el tiempo total (cuenta hacia arriba)
        timer += Time.deltaTime;

        // Le enviamos el tiempo al HUDManager para que lo formatee y lo muestre
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateTimer(timer);
        }

        if (timeText != null)
        {
            int min = Mathf.FloorToInt(timer / 60f);
            int sec = Mathf.FloorToInt(timer % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", min, sec);
        }
    }

    public void EnablePause()
    {
        if (pauseCount == 0) Time.timeScale = 0f;
        pauseCount++;
    }

    public void DisablePause()
    {
        pauseCount--;
        if (pauseCount <= 0)
        {
            pauseCount = 0;
            Time.timeScale = 1f;
            TooltipManager.Instance.HideTooltip();
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