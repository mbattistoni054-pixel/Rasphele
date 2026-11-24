using UnityEngine;

[CreateAssetMenu(fileName = "Magic Sorcerer", menuName = "ScriptableObjects/Enemy Data/Magic Sorcerer")]
public class EnemyData_MagicSorcerer : EnemyData_Base
{
    [Header("Magic Sorcerer")]

    [SerializeField] private float trackingTime;
    public float TrackingTime => trackingTime;

    [SerializeField] private float lockedTime;
    public float LockedTime => lockedTime;

    [SerializeField] private float laserDuration;
    public float LaserDuration => laserDuration;

    [SerializeField] private Material beamMaterial;
    public Material BeamMaterial => beamMaterial;

}
