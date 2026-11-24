using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class MeteorWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes del Meteorito")]
        public float heightOffset = 15f;
        public float spawnRadius = 8f;
        public LayerMask enemyLayer;

        [HideInInspector] public bool isCometMode;
        [HideInInspector] public int scatterAmount = 1;

        private Transform _cachedCameraTransform;

        protected override void Attack()
        {
            if (data == null || data.projectilePrefab == null)
            {
                attackSuccessful = false;
                return;
            }

            CurrentDamageType = isCometMode ? DamageType.Agua : data.damageType;

            int meteorsToSpawn = Mathf.Max(1, CurrentMultipleShots);
            float damagePerMeteor = GetFinalDamage();
            float bonusExplosionSize = CurrentExplosiveRadius;
            bool isScatterActive = scatterAmount > 1;

            if (isScatterActive)
            {
                meteorsToSpawn *= scatterAmount;
                damagePerMeteor /= scatterAmount;
                bonusExplosionSize /= 1.5f;
            }

            Vector3 aimDirection = GetHorizontalCameraAim();
            Vector3 fallDirection = (aimDirection + Vector3.down).normalized;

            for (int i = 0; i < meteorsToSpawn; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnOffset = new Vector3(randomCircle.x, heightOffset, randomCircle.y);
                Vector3 spawnPos = transform.position + spawnOffset - (aimDirection * (heightOffset * 0.5f));

                MeteorProjectileRefactored proj = null;

                if (ProjectilePoolManager.Instance != null)
                {
                    proj = ProjectilePoolManager.Instance.GetProjectile<MeteorProjectileRefactored>(
                        data.projectilePrefab,
                        spawnPos,
                        Quaternion.identity);
                }
                else
                {
                    GameObject meteorObj = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);
                    meteorObj.TryGetComponent(out proj);
                }

                if (proj != null)
                {
                    proj.Setup(damagePerMeteor, CurrentEffects, bonusExplosionSize, WeaponID, CurrentDamageType, enemyLayer, fallDirection, isScatterActive, isCometMode);
                }
            }
        }

        private Vector3 GetHorizontalCameraAim()
        {
            if (_cachedCameraTransform == null && Camera.main != null)
            {
                _cachedCameraTransform = Camera.main.transform;
            }

            if (_cachedCameraTransform != null)
            {
                Vector3 camForward = _cachedCameraTransform.forward;
                camForward.y = 0f;
                if (camForward.sqrMagnitude > 0.001f)
                {
                    return camForward.normalized;
                }
            }

            return transform.forward;
        }

        public override void ResetWeaponStats()
        {
            base.ResetWeaponStats();
            isCometMode = false;
            scatterAmount = 1;
        }
    }
}