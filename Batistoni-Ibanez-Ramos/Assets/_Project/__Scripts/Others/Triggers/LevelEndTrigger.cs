using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    [Header("Requisitos de la Meta")]
    [Tooltip("El nivel mínimo que debe tener el jugador para terminar el nivel.")]
    public int requiredLevel = 10;

    private void OnTriggerEnter(Collider other)
    {
        // Evitamos que se active dos veces si ya ganamos
        if (hasTriggered) return;

        // Comprobamos si el que chocó fue el jugador
        if (other.CompareTag("Player"))
        {
            // Buscamos el script de experiencia del jugador
            PlayerExperience playerXP = other.GetComponent<PlayerExperience>();

            if (playerXP != null)
            {
                // Verificamos si cumple con el nivel mínimo
                if (playerXP.currentLevel >= requiredLevel)
                {
                    hasTriggered = true;
                    Debug.Log("Nivel Completado Cumpliste el requisito de nivel.");

                    // Llamamos al GameManager para mostrar el menú de Victoria
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ShowLevelComplete();
                    }
                }
                else
                {
                    // Si no tiene el nivel, le avisamos 
                    Debug.Log($"Te falta experiencia. Eres nivel {playerXP.currentLevel}, necesitas ser nivel {requiredLevel}.");
                }
            }
        }
    }
}