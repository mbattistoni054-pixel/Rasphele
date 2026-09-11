using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class CloudWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes de Invocacin")]
        public LayerMask enemyLayer;
        public float heightOffset = 6f;

        [Header("rea de Aparicin")]
        public float minSpawnDistance = 5f;
        public float maxSpawnDistance = 15f;

        [Header("Ajustes de la Nube")]
        public float baseCloudRadius = 4f;

        protected override void Attack()
        {
            int cloudsToSpawn = CurrentMultipleShots;
            float angleStep = 360f / cloudsToSpawn;
            float randomOffset = Random.Range(0f, 360f);

            // Usamos las propiedades del Builder
            float bonusRange = GetFinalRange() - data.baseRange;
            float finalMaxDistance = Mathf.Max(minSpawnDistance + 2f, maxSpawnDistance + bonusRange);

            for (int i = 0; i < cloudsToSpawn; i++)
            {
                float angle = (i * angleStep) + randomOffset;
                Vector3 direction = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));

                float randomDist = Random.Range(minSpawnDistance, finalMaxDistance);
                Vector3 spawnPos = transform.position + (direction * randomDist) + (Vector3.up * heightOffset);

                // ! PATRN POOL: Pedimos una nube del pool
                GameObject cloudObj = ProjectilePoolManager.Instance.GetProjectile(data.projectilePrefab, spawnPos, Quaternion.identity);
                CloudEntityRefactored cloud = cloudObj.GetComponent<CloudEntityRefactored>();

                if (cloud != null)
                {
                    float finalCloudRadius = baseCloudRadius + CurrentExplosiveRadius;
                    
                    cloud.Setup(
                        GetFinalDamage(), 
                        finalCloudRadius, 
                        CurrentDuration, 
                        CurrentEffects, 
                        CurrentElectricStormChance, 
                        WeaponID, 
                        CurrentDamageType, 
                        enemyLayer
                    );
                }
            }
        }
    }
}
