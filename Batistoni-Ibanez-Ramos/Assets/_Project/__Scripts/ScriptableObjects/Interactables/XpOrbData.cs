using UnityEngine;

[CreateAssetMenu(fileName = "Data_XpOrb", menuName = "ScriptableObjects/Interactables/XpOrb")]
public class XpOrbData : ScriptableObject
{
    [SerializeField] private float magnetRadius = 5f;
    public float MagnetRadius => magnetRadius;

    [SerializeField] private float baseMoveSpeed = 10f;
    public float BaseMoveSpeed => baseMoveSpeed;
}
