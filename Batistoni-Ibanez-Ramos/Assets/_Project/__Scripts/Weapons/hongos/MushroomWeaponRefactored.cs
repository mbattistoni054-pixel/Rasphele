using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class MushroomWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes del Hongo")]
        public float spawnRadius = 8f;
        public LayerMask enemyLayer;

        private static int _cachedGroundMask = -1;

        protected override void Attack()
        {
            if (data == null || data.projectilePrefab == null)
            {
                attackSuccessful = false;
                return;
            }

            if (_cachedGroundMask == -1)
            {
                _cachedGroundMask = LayerMask.GetMask("Ground");
            }

            int mushroomsToSpawn = Mathf.Max(1, CurrentMultipleShots);
            float damagePerMushroom = GetFinalDamage();
            float sporeRadius = CurrentExplosiveRadius;
            float duration = CurrentDuration;

            for (int i = 0; i < mushroomsToSpawn; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

                if (Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, _cachedGroundMask, QueryTriggerInteraction.Ignore))
                {
                    spawnPos = hit.point;
                }

                MushroomEntityRefactored mush = null;

                if (ProjectilePoolManager.Instance != null)
                {
                    mush = ProjectilePoolManager.Instance.GetProjectile<MushroomEntityRefactored>(
                        data.projectilePrefab,
                        spawnPos,
                        Quaternion.identity);
                }
                else
                {
                    GameObject mushObj = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);
                    mushObj.TryGetComponent(out mush);
                }

                if (mush != null)
                {
                    mush.Setup(damagePerMushroom, sporeRadius, duration, CurrentEffects, CurrentHeatHeal, WeaponID, CurrentDamageType, enemyLayer);
                }
            }
        }
    }
}