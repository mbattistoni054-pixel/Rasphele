using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    /// <summary>
    /// MushroomWeaponRefactored: Integracin de los patrones Builder y Object Pool
    /// adaptados especficamente para el arma de Hongos.
    /// Hereda de WeaponBaseRealRefactored para exponer las variables al Builder.
    /// </summary>
    public class MushroomWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes del Hongo")]
        public float spawnRadius = 8f;
        public LayerMask enemyLayer;

        protected override void Attack()
        {
            // Tomamos los valores inyectados por el Builder
            int mushroomsToSpawn = CurrentMultipleShots;
            float damagePerMushroom = GetFinalDamage();
            float sporeRadius = CurrentExplosiveRadius > 0 ? CurrentExplosiveRadius : 3f;
            float duration = CurrentDuration > 0 ? CurrentDuration : 10f;

            for (int i = 0; i < mushroomsToSpawn; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

                if (Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, LayerMask.GetMask("Ground")))
                {
                    spawnPos = hit.point;
                }

                // ! PATRN POOL: Pedimos un hongo vaco del pool en lugar de Instanciar
                GameObject mushObj = ProjectilePoolManager.Instance.GetProjectile(data.projectilePrefab, spawnPos, Quaternion.identity);
                MushroomEntityRefactored mush = mushObj.GetComponent<MushroomEntityRefactored>();

                if (mush != null)
                {
                    // INYECTAMOS las estadsticas del Builder al hongo
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
