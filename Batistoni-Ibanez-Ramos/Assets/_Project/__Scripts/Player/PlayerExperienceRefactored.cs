using UnityEngine;

namespace PatronesAplicados
{
    public class PlayerExperienceRefactored : MonoBehaviour
    {
        [Header("Niveles y XP")]
        public int currentLevel = 1;
        public float currentXP = 0f;
        public float xpToNextLevel = 100f;

        [Header("Referencias UI")]
        public GameObject menuMejorasPanel;

        private void Start()
        {
            NotifyExperienceChanged();
        }

        public void AddExperience(float amount)
        {
            if (amount <= 0f) return;

            if (PlayerStatsRefactored.Instance != null)
            {
                amount *= PlayerStatsRefactored.Instance.itemXpMultiplier;
            }

            currentXP += amount;

            while (currentXP >= xpToNextLevel)
            {
                LevelUp();
            }

            NotifyExperienceChanged();
        }

        private void LevelUp()
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel = Mathf.Round(xpToNextLevel * 1.2f);

            if (menuMejorasPanel != null)
            {
                menuMejorasPanel.SetActive(true);
            }
        }

        private void NotifyExperienceChanged()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.ExperienceChanged, currentXP, xpToNextLevel, currentLevel);
            }
        }
    }
}