using UnityEngine;
using System.Collections.Generic;

public class GalvanicCoreWeapon : WeaponBase
{
    [Header("Ajustes del Núcleo Galvánico")]
    public LayerMask enemyLayer;
    public float heightOffset = 2.5f; // Altura a la que flota sobre el jugador

    [Header("Estadísticas Únicas")]
    public float maxDamageCap = 200f; // Sube con la mejora 'Daño Maximo'
    public float rampUpInterval = 1f; // Sube con la mejora 'Velocidad' (0.9, 0.8...)
    public float damageMultiplierPerTick = 1.5f; // El multiplicador base (1.5x)

    [Header("Visuales del Rayo")]
    [Tooltip("Arrastra un Material para el láser (ej: un material con shader sin luz y color cyan)")]
    public Material beamMaterial;
    public float beamWidth = 0.15f;

    // Clase interna para llevar el rastro de cada rayo activo por separado
    private class ActiveBeam
    {
        public Transform target;
        public IDamageable damageable;
        public float currentDamage;
        public float rampUpTimer;
        public float damageTickTimer;
        public LineRenderer line;
    }

    private List<ActiveBeam> activeBeams = new List<ActiveBeam>();

    protected override void Start()
    {
        base.Start();
        currentDamageType = DamageType.Electrico; // Forzamos daño eléctrico
    }

    protected override void Update()
    {
        if (data == null || PlayerStats.Instance == null) return;

        // Mantener el orbe flotando siempre arriba (asumiendo que es hijo del jugador)
        transform.localPosition = Vector3.up * heightOffset;

        MaintainBeams();
        FindNewTargets();
    }

    // Anulamos el Attack() base porque esta arma no "dispara", sino que mantiene rayos vivos
    protected override void Attack() { }

    private void MaintainBeams()
    {
        float range = GetFinalRange();

        // Recorremos al revés por si tenemos que eliminar rayos rotos
        for (int i = activeBeams.Count - 1; i >= 0; i--)
        {
            ActiveBeam beam = activeBeams[i];

            // 1. Comprobar si el objetivo huyó o murió
            // En Unity, si un enemigo muere y se destruye su GameObject, beam.target será null automáticamente.
            if (beam.target == null || beam.damageable == null ||
                Vector3.Distance(transform.position, beam.target.position) > range)
            {
                DestroyBeam(beam);
                activeBeams.RemoveAt(i);
                continue;
            }

            // 2. Actualizar las visuales del láser en tiempo real
            if (beam.line != null)
            {
                beam.line.SetPosition(0, transform.position); // Origen (Orbe)
                beam.line.SetPosition(1, beam.target.position + Vector3.up * 1f); // Fin (Pecho del enemigo)
            }

            // 3. Lógica de Daño (El "Golpe" cada 1 segundo)
            beam.damageTickTimer += Time.deltaTime;
            float tickRate = currentBaseCooldown > 0 ? currentBaseCooldown : 1f;

            if (beam.damageTickTimer >= tickRate)
            {
                beam.damageTickTimer -= tickRate;
                DealDamage(beam);
            }

            // 4. Lógica de Multiplicador (Sube el poder cada X tiempo)
            beam.rampUpTimer += Time.deltaTime;
            if (beam.rampUpTimer >= rampUpInterval)
            {
                beam.rampUpTimer -= rampUpInterval;
                beam.currentDamage *= damageMultiplierPerTick;

                if (beam.currentDamage > maxDamageCap)
                {
                    beam.currentDamage = maxDamageCap;
                }
            }
        }
    }

    private void DealDamage(ActiveBeam beam)
    {
        // "Los números decimales se clampearán" -> Lo truncamos al piso más cercano (ej: 4.5 -> 4)
        float finalDamageToApply = Mathf.Floor(beam.currentDamage);

        // Roll crítico por hit
        bool isCrit = Random.Range(0f, 100f) <= currentEffects.critChance;

        beam.damageable.TakeDamage(finalDamageToApply, isCrit, currentDamageType);
        beam.damageable.ApplyEffects(currentEffects, WeaponID);
    }

    private void FindNewTargets()
    {
        // Si ya tenemos el cupo de rayos llenos, no buscamos más
        if (activeBeams.Count >= currentMultipleShots) return;

        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, GetFinalRange(), enemyLayer);
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;
        IDamageable nearestDamageable = null;

        foreach (Collider col in enemiesInRange)
        {
            Transform enemyTransform = col.transform;

            // Verificamos que no le estemos disparando ya con otro rayo
            bool alreadyTargeted = false;
            foreach (ActiveBeam beam in activeBeams)
            {
                if (beam.target == enemyTransform)
                {
                    alreadyTargeted = true;
                    break;
                }
            }

            if (alreadyTargeted) continue;

            float distance = Vector3.Distance(transform.position, enemyTransform.position);
            if (distance < shortestDistance)
            {
                IDamageable dmg = col.GetComponent<IDamageable>();
                if (dmg != null)
                {
                    shortestDistance = distance;
                    nearestEnemy = enemyTransform;
                    nearestDamageable = dmg;
                }
            }
        }

        if (nearestEnemy != null && nearestDamageable != null)
        {
            CreateBeam(nearestEnemy, nearestDamageable);
        }
    }

    private void CreateBeam(Transform target, IDamageable damageable)
    {
        ActiveBeam newBeam = new ActiveBeam();
        newBeam.target = target;
        newBeam.damageable = damageable;

        // Inicia con el daño base (incluyendo buffos pasivos que tenga el jugador)
        newBeam.currentDamage = GetFinalDamage();
        newBeam.rampUpTimer = 0f;
        newBeam.damageTickTimer = 0f; // Empieza en 0, pegará el primer golpe tras 1 segundo

        // Crear la visual del láser por código
        GameObject lineObj = new GameObject("TeslaBeam");
        lineObj.transform.SetParent(transform);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = beamWidth;
        lr.endWidth = beamWidth;

        if (beamMaterial != null) lr.material = beamMaterial;
        else lr.material = new Material(Shader.Find("Sprites/Default")); // Material basico de Unity

        lr.startColor = Color.cyan;
        lr.endColor = Color.blue;

        newBeam.line = lr;
        activeBeams.Add(newBeam);
    }

    private void DestroyBeam(ActiveBeam beam)
    {
        if (beam.line != null)
        {
            Destroy(beam.line.gameObject);
        }
    }

    public override void ResetWeaponStats()
    {
        base.ResetWeaponStats();
        // Volver a valores de fábrica al purgar
        maxDamageCap = 200f;
        rampUpInterval = 1f;
    }
}