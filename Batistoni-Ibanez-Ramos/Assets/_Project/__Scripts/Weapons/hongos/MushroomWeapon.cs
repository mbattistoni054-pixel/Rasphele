using UnityEngine;

public class MushroomWeapon : WeaponBase
{
    [Header("Ajustes del Hongo")]
    public float spawnRadius = 8f;
    public LayerMask enemyLayer;

    protected override void Attack()
    {
        int mushroomsToSpawn = currentMultipleShots;
        float damagePerMushroom = GetFinalDamage();
        float sporeRadius = currentExplosiveRadius > 0 ? currentExplosiveRadius : 3f;
        float duration = currentDuration > 0 ? currentDuration : 10f;

        for (int i = 0; i < mushroomsToSpawn; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

 
            if (Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, LayerMask.GetMask("Ground")))
            {
                spawnPos = hit.point;
            }

            GameObject mushObj = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);
            MushroomEntity mush = mushObj.GetComponent<MushroomEntity>();

            if (mush != null)
            {

                mush.Setup(damagePerMushroom, sporeRadius, duration, currentEffects, currentHeatHeal, WeaponID, DamageType, enemyLayer);
            }
        }
    }
}