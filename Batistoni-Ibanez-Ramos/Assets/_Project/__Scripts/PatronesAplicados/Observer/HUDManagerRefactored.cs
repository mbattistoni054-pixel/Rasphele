using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PatronesAplicados
{
    public class HUDManagerRefactored : MonoBehaviour
    {
        [Header("Health UI")]
        [SerializeField] private Image hpFill;
        [SerializeField] private TextMeshProUGUI hpText;

        [Header("Level & XP UI")]
        [SerializeField] private Image xpFill;
        [SerializeField] private TextMeshProUGUI lvlText;

        [Header("Dash UI")]
        [SerializeField] private Image dashFill;

        [Header("Economy UI")]
        [SerializeField] private TextMeshProUGUI moneyText;

        [Header("Timer UI")]
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Boss UI")]
        [SerializeField] private GameObject bossPanel;
        [SerializeField] private Image bossHpFill;
        [SerializeField] private TextMeshProUGUI bossNameText;

        [Header("Interact UI")]
        [SerializeField] private TextMeshProUGUI interactText;

        private void Start()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening<float, float>("PlayerHealthChanged", UpdateHealth);
                EventManager.Instance.StartListening<int>("MoneyUpdated", UpdateMoney);
                EventManager.Instance.StartListening<float, float, int>("ExperienceChanged", UpdateExperience);
                EventManager.Instance.StartListening<float, float>("DashCooldownChanged", UpdateDashCooldown);
                
                EventManager.Instance.StartListening<string, float, float>("BossSpawned", ShowBossUI);
                EventManager.Instance.StartListening<float, float>("BossHealthChanged", UpdateBossHealth);
                EventManager.Instance.StartListening("BossDefeated", HideBossUI);
                
                EventManager.Instance.StartListening<string>("ShowInteractText", ShowInteractText);
                EventManager.Instance.StartListening<int>("TimeUpdated", UpdateTimer);
            }
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening<float, float>("PlayerHealthChanged", UpdateHealth);
                EventManager.Instance.StopListening<int>("MoneyUpdated", UpdateMoney);
                EventManager.Instance.StopListening<float, float, int>("ExperienceChanged", UpdateExperience);
                EventManager.Instance.StopListening<float, float>("DashCooldownChanged", UpdateDashCooldown);
                
                EventManager.Instance.StopListening<string, float, float>("BossSpawned", ShowBossUI);
                EventManager.Instance.StopListening<float, float>("BossHealthChanged", UpdateBossHealth);
                EventManager.Instance.StopListening("BossDefeated", HideBossUI);
                
                EventManager.Instance.StopListening<string>("ShowInteractText", ShowInteractText);
                EventManager.Instance.StopListening<int>("TimeUpdated", UpdateTimer);
            }
        }

        private void ShowInteractText(string mensaje)
        {
            if (interactText != null)
            {
                interactText.text = mensaje ?? "";
            }
        }

        private void UpdateHealth(float currentHealth, float maxHealth)
        {
            if (hpFill != null) hpFill.fillAmount = currentHealth / maxHealth;
            if (hpText != null) hpText.text = $"{Mathf.CeilToInt(currentHealth)}/{(int)maxHealth}";
        }

        private void UpdateMoney(int amount)
        {
            if (moneyText != null) moneyText.text = $"$ {amount}";
        }

        private void UpdateExperience(float currentXP, float targetXP, int currentLevel)
        {
            if (xpFill != null) xpFill.fillAmount = currentXP / targetXP;
            if (lvlText != null) lvlText.text = $"LVL {currentLevel}";
        }

        private void UpdateDashCooldown(float currentCooldown, float maxCooldown)
        {
            if (dashFill != null) dashFill.fillAmount = 1f - (currentCooldown / maxCooldown);
        }

        private void UpdateTimer(int totalSeconds)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(totalSeconds / 60f);
                int seconds = Mathf.FloorToInt(totalSeconds % 60f);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }

        private void ShowBossUI(string bossName, float currentHP, float maxHP)
        {
            if (bossPanel != null) bossPanel.SetActive(true);
            if (bossNameText != null) bossNameText.text = bossName;
            UpdateBossHealth(currentHP, maxHP);
        }

        private void UpdateBossHealth(float currentHP, float maxHP)
        {
            if (bossHpFill != null)
            {
                bossHpFill.fillAmount = currentHP / maxHP;
            }
        }

        private void HideBossUI()
        {
            if (bossPanel != null) bossPanel.SetActive(false);
        }
    }
}
