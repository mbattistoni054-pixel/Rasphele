using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPause : MonoBehaviour
{
    [SerializeField] private GameObject menuPausaCanvas; // Asigna el GameObject del Canvas Pause aquí
    [SerializeField] private GameObject optionsMenu; // Asigna el panel de opciones
    [SerializeField] private GameObject controlsMenu; // Asigna el panel de controles

    private void OnEnable()
    {
        GameManager.Instance.EnablePause();
    }

    private void OnDisable()
    {
        GameManager.Instance.DisablePause();
    }

    // Esta es la función que debes poner en el OnClick() del botón "CONTINUE"
    public void ResumeGame()
    {
        menuPausaCanvas.SetActive(false);
        // Nos aseguramos de cerrar los submenús también
        if (optionsMenu != null) optionsMenu.SetActive(false);
        if (controlsMenu != null) controlsMenu.SetActive(false);
    }



    public void ChangeScene() // Para el botón "QUIT TO MENU"
    {

        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}