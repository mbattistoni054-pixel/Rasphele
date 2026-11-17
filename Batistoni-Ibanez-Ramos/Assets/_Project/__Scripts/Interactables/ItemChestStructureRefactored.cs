using UnityEngine;
using UnityEngine.InputSystem;

namespace PatronesAplicados
{
    public class ItemChestStructureRefactored : MonoBehaviour
    {
        [Header("Configuración del Cofre")]
        public bool isSpecialChest = false;
        public int cost = 50;

        private bool _isPlayerNear;

        private void OnEnable()
        {
            _isPlayerNear = false;
        }

        private void Update()
        {
            if (_isPlayerNear && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryOpenChest();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerNear = true;

                string txt = isSpecialChest
                    ? $"Press E\n${cost}\nCofre Especial (25% Éxito)"
                    : $"Press E\n${cost}";

                ShowInteractMessage(txt);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerNear = false;
                ShowInteractMessage(string.Empty);
            }
        }

        private void TryOpenChest()
        {
            if (ItemManager.Instance == null || NEWItemRewardUI.Instance == null) return;

            bool spendSuccess = PlayerStatsRefactored.Instance != null && PlayerStatsRefactored.Instance.SpendMoney(cost);

            if (spendSuccess)
            {
                if (isSpecialChest && Random.Range(0f, 100f) > 25f)
                {
                    ShowInteractMessage($"El cofre falló al abrirse.\nPress E\n${cost}\n(25%)");
                    return;
                }

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

                _isPlayerNear = false;
                ShowInteractMessage(string.Empty);
                gameObject.SetActive(false);
            }
            else
            {
                ShowInteractMessage("No tienes suficiente dinero.");
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

            return roll <= 80f ? ItemTier.Raro : ItemTier.Extraordinario;
        }

        private static void ShowInteractMessage(string message)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.ShowInteractText, message);
            }
        }
    }
}