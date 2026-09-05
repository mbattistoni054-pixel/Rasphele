using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Rendering;

public class FountainStructure : MonoBehaviour
{
    [Header("Configuración de la Fuente")]
    public int maxUses = 3;
    private int currentUses;

    [Header("Efectos Visuales")]
    public Transform waterVisual; // El objeto azul que simula el agua
    public GameObject pressE_Text;
    public TextMeshPro floatingCostText;

    private bool isPlayerNear = false;
    private PlayerHealth playerHealth;

    void Start()
    {
        currentUses = maxUses;
        if (pressE_Text != null) pressE_Text.SetActive(false);
    }

    void Update()
    {
        // Actualizamos el costo en tiempo real si el jugador está cerca
        if (isPlayerNear && playerHealth != null)
        {
            int cost = GetHealingCost();
          
            if (cost > 0)
            {

                string txt = $"Press E\n${cost}";
                HUDManager.Instance.InteractText(txt);

            } 

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryHeal();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && currentUses > 0)
        {
            
            isPlayerNear = true;
            playerHealth = other.GetComponent<PlayerHealth>();
            if (pressE_Text != null) pressE_Text.SetActive(true);

            int cost = GetHealingCost();
            if (cost <= 0)
            {
                string txt = $"Estas sano, sin uso.";
                HUDManager.Instance.InteractText(txt);
            }
           
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerHealth = null;
            HUDManager.Instance.InteractText(null);
            if (pressE_Text != null) pressE_Text.SetActive(false);
        }
    }

    private int GetHealingCost()
    {
        if (playerHealth == null) return 0;

        // El costo es 1 a 1 con la vida faltante
        float missingHP = playerHealth.maxHealth - playerHealth.currentHealth;
        return Mathf.CeilToInt(missingHP);
    }

    private void TryHeal()
    {
        if (currentUses <= 0 || PlayerStats.Instance == null || playerHealth == null) return;

        int cost = GetHealingCost();

        if (cost <= 0)
        {
            Debug.Log("Ya tienes la vida al máximo.");
            string txt = "Ya tienes la vida al máximo";
            HUDManager.Instance.InteractText(txt);
            return;
        }

        if (PlayerStats.Instance.SpendMoney(cost))
        {
            // Curamos todo
            playerHealth.Heal(cost);
            currentUses--;

            // Bajar el agua visualmente
            if (waterVisual != null)
            {
                float fillPercent = (float)currentUses / maxUses;
                waterVisual.localScale = new Vector3(waterVisual.localScale.x, fillPercent, waterVisual.localScale.z);
            }

            string txt = $"Fuente usada. Usos restantes: {currentUses}";

            HUDManager.Instance.InteractText(txt);

            if (currentUses <= 0)
            {
               
                if (pressE_Text != null) pressE_Text.SetActive(false);
                isPlayerNear = false; // Desactivar interacciones futuras
            }
        }
        else
        {
            string txt = "No tienes dinero suficiente para curarte por completo.";

            HUDManager.Instance.InteractText(txt);
        }
    }
}