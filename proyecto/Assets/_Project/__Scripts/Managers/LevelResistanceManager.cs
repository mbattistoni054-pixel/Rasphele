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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        GenerateLevelResistances();
    }

    private void GenerateLevelResistances()
    {
        List<DamageType> allTypes = new List<DamageType>
        {
            DamageType.Magico, DamageType.Fuego,
            DamageType.Agua, DamageType.Veneno, DamageType.Electrico
        };

        for (int i = 0; i < allTypes.Count; i++)
        {
            DamageType temp = allTypes[i];
            int randomIndex = Random.Range(i, allTypes.Count);
            allTypes[i] = allTypes[randomIndex];
            allTypes[randomIndex] = temp;
        }

        weaknesses.Add(allTypes[0]);
        weaknesses.Add(allTypes[1]);

        resistances.Add(allTypes[2]);
        resistances.Add(allTypes[3]);

        normalTypes.Add(allTypes[4]);
        normalTypes.Add(DamageType.Fisico);

        Debug.Log($" Resistencias del Nivel Generadas:\nDébiles a: {weaknesses[0]}, {weaknesses[1]}\nResistentes a: {resistances[0]}, {resistances[1]}");
    }

    public float GetDamageMultiplier(DamageType typeReceived)
    {
        if (weaknesses.Contains(typeReceived)) return 2f;    
        if (resistances.Contains(typeReceived)) return 0.5f; 
        return 1f;                                           
    }
}