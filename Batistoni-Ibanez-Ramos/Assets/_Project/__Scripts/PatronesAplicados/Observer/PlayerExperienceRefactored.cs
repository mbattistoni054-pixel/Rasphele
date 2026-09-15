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

        void Start()
        {
            NotifyExperienceChanged();
        }

        public void AddExperience(float amount)
        {
            if (PlayerStatsRefactored.Instance != null)
            {
                amount *= PlayerStatsRefactored.Instance.itemXpMultiplier;
            }
            else if (PlayerStats.Instance != null)
            {
                amount *= PlayerStats.Instance.itemXpMultiplier;
            }

            currentXP += amount;


            if (currentXP >= xpToNextLevel)
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
            else
            {

            }
        }

        private void NotifyExperienceChanged()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("ExperienceChanged", currentXP, xpToNextLevel, currentLevel);
            }
        }
    }
}
