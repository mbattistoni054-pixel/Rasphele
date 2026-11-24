using UnityEngine;
using UnityEngine.InputSystem;

public class PurgadorStructure : MonoBehaviour
{
    [Header("UI y Textos")]
    public GameObject pressE_Text;

    [Header("Referencia al Canvas")]
    [Tooltip("Arrastra aquí el objeto padre del Canvas PurgadorUI que duplicaste")]
    public GameObject purgadorMenuCanvas;

    private bool isPlayerNear = false;

    void Start()
    {
        if (pressE_Text != null) pressE_Text.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (purgadorMenuCanvas != null && !purgadorMenuCanvas.activeSelf)
            {
                purgadorMenuCanvas.SetActive(true);
                if (pressE_Text != null) pressE_Text.SetActive(false); // Ocultamos la E mientras está en el menú
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (pressE_Text != null) pressE_Text.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (pressE_Text != null) pressE_Text.SetActive(false);

            // Si el jugador se aleja, cerramos el menú por si acaso
            if (purgadorMenuCanvas != null) purgadorMenuCanvas.SetActive(false);
        }
    }
}