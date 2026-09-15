using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class MushroomWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes del Hongo")]
        public float spawnRadius = 8f;
        public LayerMask enemyLayer;

        protected override void Attack()
        {
            int mushroomsToSpawn = CurrentMultipleShots;
            float damagePerMushroom = GetFinalDamage();
            float sporeRadius = CurrentExplosiveRadius;
            float duration = CurrentDuration;

            for (int i = 0; i < mushroomsToSpawn; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

                if (Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, LayerMask.GetMask("Ground")))
                {
                    spawnPos = hit.point;
                }

                GameObject mushObj = ProjectilePoolManager.Instance.GetProjectile(data.projectilePrefab, spawnPos, Quaternion.identity);
                MushroomEntityRefactored mush = mushObj.GetComponent<MushroomEntityRefactored>();

                if (mush != null)
                {
                    mush.Setup(
                        damagePerMushroom, 
                        sporeRadius, 
                        duration, 
                        CurrentEffects, 
                        CurrentHeatHeal, 
                        WeaponID, 
                        CurrentDamageType, 
                        enemyLayer
                    );
                }
            }
        }
    }
}
