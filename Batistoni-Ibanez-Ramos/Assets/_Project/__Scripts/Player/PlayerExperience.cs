using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [Header("Niveles y XP")]
    public int currentLevel = 1;
    public float currentXP = 0f;
    public float xpToNextLevel = 100f;

    [Header("Referencias UI")]
    public GameObject menuMejorasPanel;

    public delegate void ExperienceChangedHandler(float currentXP, float targetXP, int currentLevel);
    public static event ExperienceChangedHandler OnExperienceChanged;

    void Start()
    {
        OnExperienceChanged?.Invoke(currentXP, xpToNextLevel, currentLevel);
    }

    public void AddExperience(float amount)
    {
       if (PlayerStats.Instance != null)
        {
            
            amount *= PlayerStats.Instance.itemXpMultiplier;
        }

        currentXP += amount;
        Debug.Log($" +{amount} XP. Total: {currentXP}/{xpToNextLevel}");


        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        OnExperienceChanged?.Invoke(currentXP, xpToNextLevel, currentLevel);
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;

        xpToNextLevel = Mathf.Round(xpToNextLevel * 1.2f);

        Debug.Log($" ¡SUBIDA DE NIVEL! Ahora eres nivel {currentLevel}");

        // Activamos el menú
        if (menuMejorasPanel != null)
        {
            menuMejorasPanel.SetActive(true);
        }
        else
        {
            
            Debug.LogError(" ¡ERROR! Llegaste al nivel necesario pero no has asignado el 'Menu Mejoras Panel' en el inspector del Jugador.");
        }
    }
}