using UnityEngine;

[CreateAssetMenu(fileName = "Range", menuName = "ScriptableObjects/Enemy Data/Range")]
public class EnemyData_Range : EnemyData_Base
{
    [Header("Range")]

    [SerializeField] private GameObject bullet;
    public GameObject Bullet => bullet;

    [SerializeField] private int bulletSpeed = 80;
    public int BulletSpeed => bulletSpeed;

}
