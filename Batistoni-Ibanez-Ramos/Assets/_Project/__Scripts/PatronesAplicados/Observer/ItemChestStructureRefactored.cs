using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace PatronesAplicados
{
    public class ItemChestStructureRefactored : MonoBehaviour
    {
        [Header("Configuracin del Cofre")]
        public bool isSpecialChest = false;
        public int cost = 50;
        
        private bool isPlayerNear = false;

        void Update()
        {
            if (isPlayerNear && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryOpenChest();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNear = true;

                if (isSpecialChest)
                {
                    string txt = "Press E\n$50\nCofre Especial (25% Exito)";
                    if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", txt);
                }
                else
                {
                    string txt = "Press E\n$50";
                    if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", txt);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNear = false;
                if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", "");
            }
        }

        private void TryOpenChest()
        {
            if (ItemManager.Instance == null || NEWItemRewardUI.Instance == null) return;

            bool spendSuccess = false;
            if (PlayerStatsRefactored.Instance != null)
                spendSuccess = PlayerStatsRefactored.Instance.SpendMoney(cost);
            else if (PlayerStats.Instance != null)
                spendSuccess = PlayerStats.Instance.SpendMoney(cost);

            if (spendSuccess)
            {
                // Tirada de Suerte para el Cofre Especial
                if (isSpecialChest)
                {
                    if (Random.Range(0f, 100f) > 25f)
                    {
                        Debug.Log("El cofre especial fall al abrirse. Dinero perdido.");
                        string txt = "El cofre fallo al abrirse.\nPress E\n$50\n(25%)";
                        if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", txt);
                        return; // Terminamos aqu, el jugador perdi sus 50.
                    }
                }

                // Generamos el objeto
                ItemTier rolledTier = RollTier();
                ItemData rewardedItem = ItemManager.Instance.GetRandomItem(rolledTier);

                if (rewardedItem != null)
                {
                    NEWItemRewardUI.Instance.ShowReward(rewardedItem);
                }
                else
                {
                    Debug.LogWarning($"No hay objetos en la lista de {rolledTier} en el ItemManager.");
                }

                if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", "");

                Destroy(gameObject);
            }
            else
            {
                string txt = "No tienes suficiente dinero.";
                if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", txt);
            }
        }

        private ItemTier RollTier()
        {
            float roll = Random.Range(0f, 100f);

            if (!isSpecialChest)
            {
                if (roll <= 75f) return ItemTier.Comun;
                if (roll <= 95f) return ItemTier.Raro;
                return ItemTier.Extraordinario;
            }
            else
            {
                if (roll <= 80f) return ItemTier.Raro;
                return ItemTier.Extraordinario;
            }
        }
    }
}




