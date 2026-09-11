using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class MeteorWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes del Meteorito")]
        public float heightOffset = 15f;
        public float spawnRadius = 8f;
        public LayerMask enemyLayer;

        [HideInInspector] public bool isCometMode = false;
        [HideInInspector] public int scatterAmount = 1;

        protected override void Attack()
        {
            if (isCometMode) CurrentDamageType = DamageType.Agua;

            int meteorsToSpawn = CurrentMultipleShots;
            float damagePerMeteor = GetFinalDamage();
            float bonusExplosionSize = CurrentExplosiveRadius;

            if (scatterAmount > 1)
            {
                meteorsToSpawn *= scatterAmount;
                damagePerMeteor /= scatterAmount;
                bonusExplosionSize /= 1.5f;
            }

            Vector3 aimDirection = transform.forward;
            if (Camera.main != null)
            {
                aimDirection = Camera.main.transform.forward;
                aimDirection.y = 0;
                aimDirection.Normalize();
            }

            Vector3 fallDirection = (aimDirection + Vector3.down).normalized;

            for (int i = 0; i < meteorsToSpawn; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnOffset = new Vector3(randomCircle.x, heightOffset, randomCircle.y);
                Vector3 spawnPos = transform.position + spawnOffset - (aimDirection * (heightOffset * 0.5f));

                // ! PATRN POOL: Pedimos un meteorito vaco
                GameObject meteorObj = ProjectilePoolManager.Instance.GetProjectile(data.projectilePrefab, spawnPos, Quaternion.identity);
                MeteorProjectileRefactored proj = meteorObj.GetComponent<MeteorProjectileRefactored>();

                if (proj != null)
                {
                    proj.Setup(
                        damagePerMeteor, 
                        CurrentEffects, 
                        bonusExplosionSize, 
                        WeaponID, 
                        CurrentDamageType, 
                        enemyLayer, 
                        fallDirection, 
                        scatterAmount > 1, 
                        isCometMode
                    );
                }
            }
        }

        public override void ResetWeaponStats()
        {
            base.ResetWeaponStats();
            isCometMode = false;
            scatterAmount = 1;
        }
    }
}
