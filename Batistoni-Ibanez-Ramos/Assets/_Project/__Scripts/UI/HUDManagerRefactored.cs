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
                EventManager.Instance.StartListening<float, float>(GameEventKeys.PlayerHealthChanged, UpdateHealth);
                EventManager.Instance.StartListening<int>(GameEventKeys.MoneyUpdated, UpdateMoney);
                EventManager.Instance.StartListening<float, float, int>(GameEventKeys.ExperienceChanged, UpdateExperience);
                EventManager.Instance.StartListening<float, float>(GameEventKeys.DashCooldownChanged, UpdateDashCooldown);

                EventManager.Instance.StartListening<string, float, float>(GameEventKeys.BossSpawned, ShowBossUI);
                EventManager.Instance.StartListening<float, float>(GameEventKeys.BossHealthChanged, UpdateBossHealth);
                EventManager.Instance.StartListening(GameEventKeys.BossDefeated, HideBossUI);

                EventManager.Instance.StartListening<string>(GameEventKeys.ShowInteractText, ShowInteractText);
                EventManager.Instance.StartListening(GameEventKeys.HideInteractText, ClearInteractText);
                EventManager.Instance.StartListening<int>(GameEventKeys.TimeUpdated, UpdateTimer);
            }
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening<float, float>(GameEventKeys.PlayerHealthChanged, UpdateHealth);
                EventManager.Instance.StopListening<int>(GameEventKeys.MoneyUpdated, UpdateMoney);
                EventManager.Instance.StopListening<float, float, int>(GameEventKeys.ExperienceChanged, UpdateExperience);
                EventManager.Instance.StopListening<float, float>(GameEventKeys.DashCooldownChanged, UpdateDashCooldown);

                EventManager.Instance.StopListening<string, float, float>(GameEventKeys.BossSpawned, ShowBossUI);
                EventManager.Instance.StopListening<float, float>(GameEventKeys.BossHealthChanged, UpdateBossHealth);
                EventManager.Instance.StopListening(GameEventKeys.BossDefeated, HideBossUI);

                EventManager.Instance.StopListening<string>(GameEventKeys.ShowInteractText, ShowInteractText);
                EventManager.Instance.StopListening(GameEventKeys.HideInteractText, ClearInteractText);
                EventManager.Instance.StopListening<int>(GameEventKeys.TimeUpdated, UpdateTimer);
            }
        }

        private void ShowInteractText(string mensaje)
        {
            if (interactText != null)
            {
                interactText.text = mensaje ?? string.Empty;
            }
        }

        private void ClearInteractText()
        {
            if (interactText != null)
            {
                interactText.text = string.Empty;
            }
        }

        private void UpdateHealth(float currentHealth, float maxHealth)
        {
            if (hpFill != null && maxHealth > 0f) hpFill.fillAmount = currentHealth / maxHealth;
            if (hpText != null) hpText.SetText("{0}/{1}", Mathf.CeilToInt(currentHealth), (int)maxHealth);
        }

        private void UpdateMoney(int amount)
        {
            if (moneyText != null) moneyText.SetText("$ {0}", amount);
        }

        private void UpdateExperience(float currentXP, float targetXP, int currentLevel)
        {
            if (xpFill != null && targetXP > 0f) xpFill.fillAmount = currentXP / targetXP;
            if (lvlText != null) lvlText.SetText("LVL {0}", currentLevel);
        }

        private void UpdateDashCooldown(float currentCooldown, float maxCooldown)
        {
            if (dashFill != null && maxCooldown > 0f)
            {
                dashFill.fillAmount = 1f - (currentCooldown / maxCooldown);
            }
        }

        private void UpdateTimer(int totalSeconds)
        {
            if (timerText != null)
            {
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                timerText.SetText("{0:00}:{1:00}", minutes, seconds);
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
            if (bossHpFill != null && maxHP > 0f)
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