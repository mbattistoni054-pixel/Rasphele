using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class CloudWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes de Invocación")]
        public LayerMask enemyLayer;
        public float heightOffset = 6f;

        [Header("Área de Aparición")]
        public float minSpawnDistance = 5f;
        public float maxSpawnDistance = 15f;

        [Header("Ajustes de la Nube")]
        public float baseCloudRadius = 4f;

        protected override void Attack()
        {
            if (data == null || data.projectilePrefab == null)
            {
                attackSuccessful = false;
                return;
            }

            int cloudsToSpawn = Mathf.Max(1, CurrentMultipleShots);
            float angleStep = 360f / cloudsToSpawn;
            float randomOffset = Random.Range(0f, 360f);

            float bonusRange = GetFinalRange() - data.baseRange;
            float finalMaxDistance = Mathf.Max(minSpawnDistance + 2f, maxSpawnDistance + bonusRange);
            float finalCloudRadius = baseCloudRadius + CurrentExplosiveRadius;
            float finalDamage = GetFinalDamage();

            for (int i = 0; i < cloudsToSpawn; i++)
            {
                float angleRad = ((i * angleStep) + randomOffset) * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad));

                float randomDist = Random.Range(minSpawnDistance, finalMaxDistance);
                Vector3 spawnPos = transform.position + (direction * randomDist) + (Vector3.up * heightOffset);

                CloudEntityRefactored cloud = null;

                if (ProjectilePoolManager.Instance != null)
                {
                    cloud = ProjectilePoolManager.Instance.GetProjectile<CloudEntityRefactored>(
                        data.projectilePrefab,
                        spawnPos,
                        Quaternion.identity);
                }
                else
                {
                    GameObject cloudObj = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);
                    cloudObj.TryGetComponent(out cloud);
                }

                if (cloud != null)
                {
                    cloud.Setup(finalDamage, finalCloudRadius, CurrentDuration, CurrentEffects, CurrentElectricStormChance, WeaponID, CurrentDamageType, enemyLayer);
                }
            }
        }
    }
}