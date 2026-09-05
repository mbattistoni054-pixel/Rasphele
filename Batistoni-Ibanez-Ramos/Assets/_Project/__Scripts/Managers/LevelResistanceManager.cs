using UnityEngine;
using System.Collections.Generic;


public class LevelResistanceManager : MonoBehaviour
{
    public static LevelResistanceManager Instance;

    [Header("Información del Nivel (Solo Lectura)")]
    public List<DamageType> weaknesses = new List<DamageType>();
    public List<DamageType> resistances = new List<DamageType>();
    public List<DamageType> normalTypes = new List<DamageType>();

    private void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        GenerateLevelResistances();
    }

    private void GenerateLevelResistances()
    {
        // 1. Metemos todos los tipos de daño en una lista
        List<DamageType> allTypes = new List<DamageType>
        {
            DamageType.Magico, DamageType.Fuego,
            DamageType.Agua, DamageType.Veneno, DamageType.Electrico
        };

        // 2. Barajamos la lista aleatoriamente (Shuffle)
        for (int i = 0; i < allTypes.Count; i++)
        {
            DamageType temp = allTypes[i];
            int randomIndex = Random.Range(i, allTypes.Count);
            allTypes[i] = allTypes[randomIndex];
            allTypes[randomIndex] = temp;
        }

        // 3. Repartimos: 2 Débiles, 2 Resistentes, 2 Normales
        weaknesses.Add(allTypes[0]);
        weaknesses.Add(allTypes[1]);

        resistances.Add(allTypes[2]);
        resistances.Add(allTypes[3]);

        normalTypes.Add(allTypes[4]);
        normalTypes.Add(DamageType.Fisico);

        Debug.Log($" Resistencias del Nivel Generadas:\nDébiles a: {weaknesses[0]}, {weaknesses[1]}\nResistentes a: {resistances[0]}, {resistances[1]}");
    }

    // Función que usarán los enemigos cuando reciban un golpe
    public float GetDamageMultiplier(DamageType typeReceived)
    {
        if (weaknesses.Contains(typeReceived)) return 2f;    // Doble daño
        if (resistances.Contains(typeReceived)) return 0.5f; // Mitad de daño
        return 1f;                                           // Daño normal
    }
}