using UnityEngine;
using System.Collections.Generic;

namespace PatronesAplicados.RealImplementation
{
    /// <summary>
    /// GalvanicCoreWeaponRefactored: Adaptacin del Ncleo Galvnico.
    /// - Utiliza el WeaponBuilder para heredar sus estadsticas sin castings feos en el UpgradeManager.
    /// - Implementa un Patrn de Object Pool LOCAL para los LineRenderers, evitando usar "new GameObject()" 
    ///   y "Destroy()" cada vez que cambia de enemigo.
    /// </summary>
    public class GalvanicCoreWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes del Ncleo Galvnico")]
        public LayerMask enemyLayer;
        public float heightOffset = 2.5f;

        [Header("Estadsticas nicas Base")]
        public float damageMultiplierPerTick = 1.5f;

        [Header("Visuales del Rayo")]
        public Material beamMaterial;
        public float beamWidth = 0.15f;

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
        
        // ! PATRN POOL (Local): Cola para guardar los LineRenderers apagados
        private Queue<LineRenderer> lineRendererPool = new Queue<LineRenderer>();

        protected override void Start()
        {
            base.Start();
            CurrentDamageType = DamageType.Electrico;
        }

        protected override void Update()
        {
            if (data == null) return;

            currentCooldownTimer += Time.deltaTime;
            float actualCooldown = CurrentBaseCooldown * cachedGlobalFireRateMult;

            transform.localPosition = Vector3.up * heightOffset;

            MaintainBeams();
            FindNewTargets();
        }

        protected override void Attack() { }

        private void MaintainBeams()
        {
            float range = GetFinalRange();

            for (int i = activeBeams.Count - 1; i >= 0; i--)
            {
                ActiveBeam beam = activeBeams[i];

                if (beam.target == null || beam.damageable == null || !beam.target.gameObject.activeInHierarchy ||
                    Vector3.Distance(transform.position, beam.target.position) > range)
                {
                    DestroyBeam(beam);
                    activeBeams.RemoveAt(i);
                    continue;
                }

                if (beam.line != null)
                {
                    beam.line.SetPosition(0, transform.position);
                    beam.line.SetPosition(1, beam.target.position + Vector3.up * 1f);
                }

                beam.damageTickTimer += Time.deltaTime;
                float tickRate = CurrentBaseCooldown;

                if (beam.damageTickTimer >= tickRate)
                {
                    beam.damageTickTimer -= tickRate;
                    DealDamage(beam);
                }

                beam.rampUpTimer += Time.deltaTime;
                // Usamos la variable inyectada por el Builder
                if (beam.rampUpTimer >= CurrentRampUpInterval)
                {
                    beam.rampUpTimer -= CurrentRampUpInterval;
                    beam.currentDamage *= damageMultiplierPerTick;

                    // Usamos la variable inyectada por el Builder
                    if (beam.currentDamage > CurrentMaxDamageCap)
                    {
                        beam.currentDamage = CurrentMaxDamageCap;
                    }
                }
            }
        }

        private void DealDamage(ActiveBeam beam)
        {
            float finalDamageToApply = Mathf.Floor(beam.currentDamage);
            bool isCrit = Random.Range(0f, 100f) <= CurrentEffects.critChance;

            beam.damageable.TakeDamage(finalDamageToApply, isCrit, CurrentDamageType);
            beam.damageable.ApplyEffects(CurrentEffects, WeaponID);
        }

        private void FindNewTargets()
        {
            if (activeBeams.Count >= CurrentMultipleShots) return;

            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, GetFinalRange(), enemyLayer);
            float shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;
            IDamageable nearestDamageable = null;

            foreach (Collider col in enemiesInRange)
            {
                Transform enemyTransform = col.transform;
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
            newBeam.currentDamage = GetFinalDamage();
            newBeam.rampUpTimer = 0f;
            newBeam.damageTickTimer = 0f;

            // ! USO DEL POOL LOCAL
            LineRenderer lr = GetLineRendererFromPool();
            newBeam.line = lr;
            
            activeBeams.Add(newBeam);
        }

        private void DestroyBeam(ActiveBeam beam)
        {
            if (beam.line != null)
            {
                // ! RETORNO AL POOL LOCAL EN VEZ DE DESTROY
                ReturnLineRendererToPool(beam.line);
            }
        }

        // --- LGICA DEL POOL LOCAL ---

        private LineRenderer GetLineRendererFromPool()
        {
            if (lineRendererPool.Count > 0)
            {
                LineRenderer lr = lineRendererPool.Dequeue();
                lr.gameObject.SetActive(true);
                return lr;
            }
            else
            {
                // Si el pool est vaco, instanciamos uno nuevo
                GameObject lineObj = new GameObject("TeslaBeam_Pooled");
                lineObj.transform.SetParent(transform);
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                
                lr.positionCount = 2;
                lr.startWidth = beamWidth;
                lr.endWidth = beamWidth;

                if (beamMaterial != null) lr.material = beamMaterial;
                else lr.material = new Material(Shader.Find("Sprites/Default"));

                lr.startColor = Color.cyan;
                lr.endColor = Color.blue;

                return lr;
            }
        }

        private void ReturnLineRendererToPool(LineRenderer lr)
        {
            lr.gameObject.SetActive(false);
            lineRendererPool.Enqueue(lr);
        }

        public override void ResetWeaponStats()
        {
            base.ResetWeaponStats();
            CurrentMaxDamageCap = 200f;
            CurrentRampUpInterval = 1f;
        }
    }
}
