using UnityEngine;
using TMPro;

namespace PatronesAplicados
{
    /// <summary>
    /// HealthUIObserver: Ejemplo de clase concreta que observa la vida del jugador.
    /// Se registra con el PlayerHealthRefactored para actualizar su UI automticamente.
    /// </summary>
    public class HealthUIObserver : MonoBehaviour, IObserver
    {
        [Header("Referencias")]
        // Referencia al Sujeto (El Player)
        public PlayerHealthRefactored playerSubject;
        
        // UI que queremos actualizar
        public TextMeshProUGUI healthText;
        public UnityEngine.UI.Slider healthSlider;

        private void OnEnable()
        {
            // Al activarse la UI, nos registramos como observadores del jugador
            if (playerSubject != null)
            {
                playerSubject.RegisterObserver(this);
            }
        }

        private void OnDisable()
        {
            // Es vital desuscribirse para evitar Memory Leaks o NullReferenceExceptions
            if (playerSubject != null)
            {
                playerSubject.RemoveObserver(this);
            }
        }

        // --- IMPLEMENTACIN DE IOBSERVER ---
        
        /// <summary>
        /// Este mtodo es llamado por PlayerHealthRefactored (el Sujeto) cada vez que sufre dao o se cura.
        /// </summary>
        public void OnNotify(float currentHealth, float maxHealth)
        {
            // Actualizamos la UI con los nuevos datos
            if (healthText != null)
            {
                healthText.text = $"HP: {currentHealth}/{maxHealth}";
            }

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }

            Debug.Log("HealthUIObserver: UI Actualizada tras recibir notificacin del Sujeto.");
        }
    }
}
