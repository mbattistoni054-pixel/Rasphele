using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPause : MonoBehaviour
{
    [SerializeField] private GameObject menuPausaCanvas;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject controlsMenu;

    private void OnEnable()
    {
        if (GameManager.Instance != null) GameManager.Instance.EnablePause();
        else if (PatronesAplicados.EventManager.Instance != null) PatronesAplicados.EventManager.Instance.TriggerEvent("PauseRequested");
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.DisablePause();
        else if (PatronesAplicados.EventManager.Instance != null) PatronesAplicados.EventManager.Instance.TriggerEvent("ResumeRequested");
    }

    public void ResumeGame()
    {
        menuPausaCanvas.SetActive(false);
        if (optionsMenu != null) optionsMenu.SetActive(false);
        if (controlsMenu != null) controlsMenu.SetActive(false);
    }

    public void ChangeScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}