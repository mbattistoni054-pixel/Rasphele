using PatronesAplicados;
using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    [Header("Requisitos de la Meta")]
    [Tooltip("El nivel mínimo que debe tener el jugador para terminar el nivel.")]
    public int requiredLevel = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            PlayerExperienceRefactored playerXP = other.GetComponent<PlayerExperienceRefactored>();

            if (playerXP != null)
            {
                if (playerXP.currentLevel >= requiredLevel)
                {
                    hasTriggered = true;
                    Debug.Log("Nivel Completado Cumpliste el requisito de nivel.");

                    if (GameManagerRefactored.Instance != null)
                    {
                        GameManagerRefactored.Instance.ShowLevelComplete();
                    }
                }
                else
                {
                    Debug.Log($"Te falta experiencia. Eres nivel {playerXP.currentLevel}, necesitas ser nivel {requiredLevel}.");
                }
            }
        }
    }
}