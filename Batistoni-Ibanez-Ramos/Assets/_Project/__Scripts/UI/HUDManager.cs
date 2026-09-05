using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

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

    public TextMeshProUGUI interactText; 

    public void InteractText(string mensaje = "")
    {
        if (interactText != null)
        {
            interactText.text = mensaje;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHealth;
        PlayerStats.OnMoneyChanged += UpdateMoney;
        PlayerExperience.OnExperienceChanged += UpdateExperience;
        CopiaPlayerController2.OnDashCooldownChanged += UpdateDashCooldown;

        BossPlant.OnBossSpawned += ShowBossUI;
        BossPlant.OnBossHealthChanged += UpdateBossHealth;
        BossPlant.OnBossDefeated += HideBossUI;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateHealth;
        PlayerStats.OnMoneyChanged -= UpdateMoney;
        PlayerExperience.OnExperienceChanged -= UpdateExperience;
        CopiaPlayerController2.OnDashCooldownChanged -= UpdateDashCooldown;

        BossPlant.OnBossSpawned -= ShowBossUI;
        BossPlant.OnBossHealthChanged -= UpdateBossHealth;
        BossPlant.OnBossDefeated -= HideBossUI;
    }

    // Como esta función tiene los mismos parámetros (float, float), ¡encaja perfecto en el evento!
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        hpFill.fillAmount = currentHealth / maxHealth;
        hpText.text = $"{Mathf.CeilToInt(currentHealth)}/{maxHealth}";
    }

    // También encaja perfecto en el evento de dinero (int)
    public void UpdateMoney(int amount)
    {
        if (moneyText != null)
        {
            moneyText.text = $"$ {amount}";
        }
    }

    public void UpdateExperience(float currentXP, float targetXP, int currentLevel)
    {
        xpFill.fillAmount = currentXP / targetXP;
        lvlText.text = $"LVL {currentLevel}";
    }

    public void UpdateDashCooldown(float currentCooldown, float maxCooldown)
    {
        dashFill.fillAmount = 1f - (currentCooldown / maxCooldown);
    }

    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
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