using UnityEngine;
using System.Collections.Generic;

public class ShurikenWeapon : WeaponBase
{
    [Header("Ajustes del Shuriken")]
    public LayerMask enemyLayer;
    public float orbitRadius = 2f;    // A qué distancia del jugador giran
    public float orbitSpeed = 120f;   // Qué tan rápido giran alrededor tuyo

    [HideInInspector] public bool isMagicMode = false;

    private List<ShurikenProjectile> shurikens = new List<ShurikenProjectile>();
    private float currentOrbitAngle = 0f;
    private float internalFireTimer = 0f;

    protected override void Start()
    {
        base.Start();
        UpdateShurikenCount();
    }

    protected override void Update()
    {
        if (data == null || PlayerStats.Instance == null) return;

        // Efecto Mágico (Cambia el daño en tiempo real si agarras la carta)
        currentDamageType = isMagicMode ? DamageType.Magico : data.damageType;

        // Controlar la cantidad (Se crean más si agarras MultipleShots)
        UpdateShurikenCount();

        // Matemáticas para hacer que orbiten uniformemente
        currentOrbitAngle += orbitSpeed * Time.deltaTime;
        if (currentOrbitAngle >= 360f) currentOrbitAngle -= 360f;

        float angleStep = 360f / shurikens.Count;

        for (int i = 0; i < shurikens.Count; i++)
        {
            if (shurikens[i] != null)
            {
                // Calculamos su posición ideal en el círculo alrededor del jugador
                float angle = currentOrbitAngle + (i * angleStep);
                Vector3 offset = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)) * orbitRadius;
                Vector3 targetOrbitPos = transform.position + Vector3.up * 1f + offset; // Vector.up para que floten por la cintura

                // Le mandamos la posición a cada shuriken
                shurikens[i].UpdateOrbitPosition(targetOrbitPos);
            }
        }

        // Lógica de Disparo Continua (El Boomerang dispara siempre que puede)
        internalFireTimer += Time.deltaTime;

        // Disparamos con un ínfimo retraso (0.05s) para que si tienes 5 shurikens, 
        // salgan como una ametralladora hermosa y no todos apilados en el mismo milisegundo.
        if (internalFireTimer >= 0.05f)
        {
            Attack();
            internalFireTimer = 0f;
        }
    }

    protected override void Attack()
    {
        Transform nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
        {
            // No hay enemigos a la vista
            attackSuccessful = false;
            return;
        }

        bool fired = false;

        // Buscamos un shuriken que esté "Idle" (Dando vueltas sin hacer nada) y lo mandamos a atacar
        foreach (var shuriken in shurikens)
        {
            if (shuriken != null && shuriken.IsIdle)
            {
                shuriken.Fire(nearestEnemy, GetFinalDamage(), currentEffects, WeaponID, currentDamageType, enemyLayer, currentProjectileSpeed);
                break; // Solo disparamos uno por frame
            }
        }

        if (!fired) attackSuccessful = false;

    }

    private void UpdateShurikenCount()
    {
        if (data == null || data.projectilePrefab == null) return;

        // Limpiamos los que hayan podido quedar nulos (si se destruyeron accidentalmente)
        shurikens.RemoveAll(s => s == null);

        // Si tenemos menos shurikens que nuestra estadística Múltiple, creamos más
        while (shurikens.Count < currentMultipleShots)
        {
            GameObject obj = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);
            ShurikenProjectile proj = obj.GetComponent<ShurikenProjectile>();
            if (proj != null) shurikens.Add(proj);
            else Destroy(obj);
        }

        // Si tenemos demasiados (por ejemplo, pasaste por el Purgador), destruimos los extras
        while (shurikens.Count > currentMultipleShots)
        {
            int lastIndex = shurikens.Count - 1;
            if (shurikens[lastIndex] != null) Destroy(shurikens[lastIndex].gameObject);
            shurikens.RemoveAt(lastIndex);
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, GetFinalRange(), enemyLayer);
        float shortestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (Collider col in enemiesInRange)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < shortestDist)
            {
                shortestDist = dist;
                nearest = col.transform;
            }
        }
        return nearest;
    }

    public override void ResetWeaponStats()
    {
        base.ResetWeaponStats();
        isMagicMode = false;
    }
}