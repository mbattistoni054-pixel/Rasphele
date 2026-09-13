using UnityEngine;
using TMPro;
using PatronesAplicados;

public class NEWCanvasLookCamera : MonoBehaviour
{
    public TextMeshProUGUI useText; 

    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StartListening<string>("ShowInteractText", ShowText);
        }
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StopListening<string>("ShowInteractText", ShowText);
        }
    }

    private void Start()
    {
        // Si el EventManager tarda en cargar, nos aseguramos de suscribirnos ac tambin
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StartListening<string>("ShowInteractText", ShowText);
        }

        if (useText != null)
        {
            useText.text = "";
        }
    }

    void Update()
    {
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }

    private void ShowText(string text)
    {
        if (useText != null)
        {
            useText.text = text;
        }
    }
}
