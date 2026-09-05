using UnityEngine;

public class MeteorWeapon : WeaponBase
{
    [Header("Ajustes del Meteorito")]
    public float heightOffset = 15f;
    public float spawnRadius = 8f;
    public LayerMask enemyLayer;

    [HideInInspector] public bool isCometMode = false;
    [HideInInspector] public int scatterAmount = 1;

    protected override void Attack()
    {
        if (isCometMode) currentDamageType = DamageType.Agua;

        int meteorsToSpawn = currentMultipleShots;
        float damagePerMeteor = GetFinalDamage();
        float bonusExplosionSize = currentExplosiveRadius;

        if (scatterAmount > 1)
        {
            meteorsToSpawn *= scatterAmount;
            damagePerMeteor /= scatterAmount;
            bonusExplosionSize /= 1.5f;
        }

        Vector3 aimDirection = transform.forward; // Por si la cámara falla, usamos el frente
        if (Camera.main != null)
        {
            aimDirection = Camera.main.transform.forward;
            // Ponemos la Y en 0 para que la cámara no afecte la caída si miras muy arriba o abajo
            aimDirection.y = 0;
            aimDirection.Normalize();
        }

        // Sumar "Hacia Adelante" + "Hacia Abajo" nos da un ángulo perfecto de -45 grados
        Vector3 fallDirection = (aimDirection + Vector3.down).normalized;

        for (int i = 0; i < meteorsToSpawn; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

            //  Restamos un poco de la dirección de la cámara a la posición de aparición.
            // Esto hace que nazcan a tus espaldas, en lo alto del cielo, y caigan hacia donde miras.
            Vector3 spawnOffset = new Vector3(randomCircle.x, heightOffset, randomCircle.y);
            Vector3 spawnPos = transform.position + spawnOffset - (aimDirection * (heightOffset * 0.5f));

            GameObject meteorObj = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);
            MeteorProjectile proj = meteorObj.GetComponent<MeteorProjectile>();

            if (proj != null)
            {
                proj.Setup(damagePerMeteor, currentEffects, bonusExplosionSize, WeaponID, DamageType, enemyLayer, fallDirection, scatterAmount > 1, isCometMode);
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