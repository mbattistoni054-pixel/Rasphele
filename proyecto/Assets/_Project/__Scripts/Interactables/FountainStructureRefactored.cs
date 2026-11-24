using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace PatronesAplicados
{
    public class FountainStructureRefactored : MonoBehaviour
    {
        [Header("Configuración de la Fuente")]
        public int maxUses = 3;
        private int _currentUses;

        [Header("Efectos Visuales")]
        public Transform waterVisual;
        public GameObject pressE_Text;
        public TextMeshPro floatingCostText;

        private bool _isPlayerNear;
        private PlayerHealthRefactored _playerHealth;
        private int _lastDisplayedCost = -1;

        private void OnEnable()
        {
            _currentUses = maxUses;
            _isPlayerNear = false;
            _playerHealth = null;
            _lastDisplayedCost = -1;

            if (pressE_Text != null) pressE_Text.SetActive(false);
            if (waterVisual != null)
            {
                Vector3 scale = waterVisual.localScale;
                waterVisual.localScale = new Vector3(scale.x, 1f, scale.z);
            }
        }

        private void Start()
        {
            _currentUses = maxUses;
            if (pressE_Text != null) pressE_Text.SetActive(false);
        }

        private void Update()
        {
            if (!_isPlayerNear || _playerHealth == null) return;

            int cost = GetHealingCost();

            if (cost != _lastDisplayedCost)
            {
                _lastDisplayedCost = cost;
                if (cost > 0)
                {
                    ShowInteractMessage($"Press E\n${cost}");
                }
                else
                {
                    ShowInteractMessage("Estás sano, sin uso.");
                }
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryHeal();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _currentUses > 0)
            {
                _isPlayerNear = true;
                _lastDisplayedCost = -1;
                other.TryGetComponent(out _playerHealth);

                if (pressE_Text != null) pressE_Text.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerNear = false;
                _playerHealth = null;
                _lastDisplayedCost = -1;

                ShowInteractMessage(string.Empty);
                if (pressE_Text != null) pressE_Text.SetActive(false);
            }
        }

        private int GetHealingCost()
        {
            if (_playerHealth == null) return 0;

            float missingHP = _playerHealth.maxHealth - _playerHealth.currentHealth;
            return Mathf.CeilToInt(missingHP);
        }

        private void TryHeal()
        {
            if (_currentUses <= 0 || _playerHealth == null) return;

            int cost = GetHealingCost();

            if (cost <= 0)
            {
                ShowInteractMessage("Ya tienes la vida al máximo.");
                return;
            }

            bool spendSuccess = PlayerStatsRefactored.Instance != null && PlayerStatsRefactored.Instance.SpendMoney(cost);

            if (spendSuccess)
            {
                _playerHealth.Heal(cost);
                _currentUses--;
                _lastDisplayedCost = GetHealingCost();

                if (waterVisual != null && maxUses > 0)
                {
                    float fillPercent = (float)_currentUses / maxUses;
                    Vector3 scale = waterVisual.localScale;
                    waterVisual.localScale = new Vector3(scale.x, fillPercent, scale.z);
                }

                ShowInteractMessage($"Fuente usada. Usos restantes: {_currentUses}");

                if (_currentUses <= 0)
                {
                    if (pressE_Text != null) pressE_Text.SetActive(false);
                    _isPlayerNear = false;
                }
            }
            else
            {
                ShowInteractMessage("No tienes dinero suficiente para curarte por completo.");
            }
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