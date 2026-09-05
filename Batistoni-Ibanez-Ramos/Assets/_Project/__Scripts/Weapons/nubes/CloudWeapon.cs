using UnityEngine;

public class CloudWeapon : WeaponBase
{
    [Header("Ajustes de Invocación")]
    public LayerMask enemyLayer;
    public float heightOffset = 6f; // Altura en el cielo

    [Header("Área de Aparición")]
    public float minSpawnDistance = 5f; // Distancia mínima para que NO caigan sobre el jugador
    public float maxSpawnDistance = 15f; // Distancia máxima base a la que pueden llegar

    [Header("Ajustes de la Nube")]
    public float baseCloudRadius = 4f; // Tamaño base del círculo rojo

    protected override void Attack()
    {
        int cloudsToSpawn = currentMultipleShots;

        // Dividimos los 360 grados en partes iguales para que nunca se amontonen
        float angleStep = 360f / cloudsToSpawn;

        // Le damos un giro aleatorio inicial para que no aparezcan siempre en las mismas direcciones
        float randomOffset = Random.Range(0f, 360f);

        // Calculamos el Rango Extra obtenido por las mejoras (Cartas de Rango)
        float bonusRange = GetFinalRange() - data.baseRange;
        // La distancia máxima final es nuestra variable base + el bono de las mejoras
        float finalMaxDistance = Mathf.Max(minSpawnDistance + 2f, maxSpawnDistance + bonusRange);

        for (int i = 0; i < cloudsToSpawn; i++)
        {
            // Calculamos la dirección exacta para esta nube
            float angle = (i * angleStep) + randomOffset;
            Vector3 direction = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));

            // Elegimos una distancia totalmente aleatoria entre tu Mínimo y tu Máximo
            float randomDist = Random.Range(minSpawnDistance, finalMaxDistance);

            // Posición final
            Vector3 spawnPos = transform.position + (direction * randomDist) + (Vector3.up * heightOffset);

            // Invocamos
            GameObject cloudObj = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);
            CloudEntity cloud = cloudObj.GetComponent<CloudEntity>();

            if (cloud != null)
            {
                // Sumamos el radio base con la estadística de Área (ExplosiveRadius)
                float finalCloudRadius = baseCloudRadius + CurrentExplosiveRadius;

                cloud.Setup(GetFinalDamage(), finalCloudRadius, currentDuration, currentEffects, currentElectricStormChance, WeaponID, DamageType, enemyLayer);
            }
        }
    }
}