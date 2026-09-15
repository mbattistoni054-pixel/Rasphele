using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace PatronesAplicados
{
    public class FountainStructureRefactored : MonoBehaviour
    {
        [Header("Configuracin de la Fuente")]
        public int maxUses = 3;
        private int currentUses;

        [Header("Efectos Visuales")]
        public Transform waterVisual; 
        public GameObject pressE_Text;
        public TextMeshPro floatingCostText;

        private bool isPlayerNear = false;
        private PlayerHealthRefactored playerHealth;

        void Start()
        {
            currentUses = maxUses;
            if (pressE_Text != null) pressE_Text.SetActive(false);
        }

        void Update()
        {
            if (isPlayerNear && playerHealth != null)
            {
                int cost = GetHealingCost();
              
                if (cost > 0)
                {
                    string txt = $"Press E\n${cost}";
                    if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", txt);
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
                playerHealth = other.GetComponent<PlayerHealthRefactored>();
                if (pressE_Text != null) pressE_Text.SetActive(true);

                int cost = GetHealingCost();
                if (cost <= 0)
                {
                    string txt = $"Estas sano, sin uso.";
                    if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", txt);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNear = false;
                playerHealth = null;
                if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", "");
                if (pressE_Text != null) pressE_Text.SetActive(false);
            }
        }

        private int GetHealingCost()
        {
            if (playerHealth == null) return 0;

            float missingHP = playerHealth.maxHealth - playerHealth.currentHealth;
            return Mathf.CeilToInt(missingHP);
        }

        private void TryHeal()
        {
            bool spendSuccess = false;
            if (PlayerStatsRefactored.Instance != null)
                spendSuccess = PlayerStatsRefactored.Instance.SpendMoney(GetHealingCost());
            else if (PlayerStats.Instance != null)
                spendSuccess = PlayerStats.Instance.SpendMoney(GetHealingCost());

            if (currentUses <= 0 || playerHealth == null) return;

            int cost = GetHealingCost();

            if (cost <= 0)
            {
                Debug.Log("Ya tienes la vida al mximo.");
                string txt = "Ya tienes la vida al mximo";
                if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", txt);
                return;
            }

            if (spendSuccess)
            {
                playerHealth.Heal(cost);
                currentUses--;

                if (waterVisual != null)
                {
                    float fillPercent = (float)currentUses / maxUses;
                    waterVisual.localScale = new Vector3(waterVisual.localScale.x, fillPercent, waterVisual.localScale.z);
                }

                string txt = $"Fuente usada. Usos restantes: {currentUses}";
                if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", txt);

                if (currentUses <= 0)
                {
                    if (pressE_Text != null) pressE_Text.SetActive(false);
                    isPlayerNear = false; 
                }
            }
            else
            {
                string txt = "No tienes dinero suficiente para curarte por completo.";
                if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", txt);
            }
        }
    }
}




