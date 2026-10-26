using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class GalvanicCoreWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes del Núcleo Galvánico")]
        public LayerMask enemyLayer;
        public float heightOffset = 2.5f;

        [Header("Estadísticas Únicas Base")]
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

        private readonly List<ActiveBeam> _activeBeams = new List<ActiveBeam>(8);
        private readonly Queue<LineRenderer> _lineRendererPool = new Queue<LineRenderer>();

        private static readonly Collider[] HitBuffer = new Collider[64];
        private static Material _fallbackBeamMaterial;

        protected override void Start()
        {
            base.Start();
            CurrentDamageType = DamageType.Electrico;
        }

        protected override void Update()
        {
            if (data == null || isPaused) return;

            transform.localPosition = Vector3.up * heightOffset;

            MaintainBeams(Time.deltaTime);
            FindNewTargets();
        }

        protected override void Attack() { }

        private void MaintainBeams(float dt)
        {
            float range = GetFinalRange();
            float rangeSqr = range * range;
            Vector3 myPos = transform.position;
            float tickRate = CurrentBaseCooldown * cachedGlobalFireRateMult;

            for (int i = _activeBeams.Count - 1; i >= 0; i--)
            {
                ActiveBeam beam = _activeBeams[i];

                if (beam.target == null || beam.damageable == null || !beam.target.gameObject.activeInHierarchy ||
                    (beam.target.position - myPos).sqrMagnitude > rangeSqr)
                {
                    DestroyBeam(beam);
                    _activeBeams.RemoveAt(i);
                    continue;
                }

                if (beam.line != null)
                {
                    beam.line.SetPosition(0, myPos);
                    beam.line.SetPosition(1, beam.target.position + Vector3.up);
                }

                beam.damageTickTimer += dt;
                if (beam.damageTickTimer >= tickRate)
                {
                    beam.damageTickTimer -= tickRate;
                    DealDamage(beam);
                }

                beam.rampUpTimer += dt;
                if (beam.rampUpTimer >= CurrentRampUpInterval)
                {
                    beam.rampUpTimer -= CurrentRampUpInterval;
                    beam.currentDamage = Mathf.Min(CurrentMaxDamageCap, beam.currentDamage * damageMultiplierPerTick);
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
            if (_activeBeams.Count >= CurrentMultipleShots) return;

            Vector3 myPos = transform.position;
            int count = Physics.OverlapSphereNonAlloc(myPos, GetFinalRange(), HitBuffer, enemyLayer);

            float shortestSqrDistance = Mathf.Infinity;
            Transform nearestEnemy = null;
            IDamageable nearestDamageable = null;

            for (int i = 0; i < count; i++)
            {
                Collider col = HitBuffer[i];
                if (col == null || !col.gameObject.activeInHierarchy) continue;

                if (!DamageableRegistry.TryGet(col, out IDamageable dmg, out Transform enemyRoot)) continue;
                if (enemyRoot == null || !enemyRoot.gameObject.activeInHierarchy) continue;

                if (IsTargetAlreadyInBeam(enemyRoot)) continue;

                float sqrDistance = (enemyRoot.position - myPos).sqrMagnitude;
                if (sqrDistance < shortestSqrDistance)
                {
                    shortestSqrDistance = sqrDistance;
                    nearestEnemy = enemyRoot;
                    nearestDamageable = dmg;
                }
            }

            if (nearestEnemy != null && nearestDamageable != null)
            {
                CreateBeam(nearestEnemy, nearestDamageable);
            }
        }

        private bool IsTargetAlreadyInBeam(Transform candidate)
        {
            for (int i = 0; i < _activeBeams.Count; i++)
            {
                if (_activeBeams[i].target == candidate) return true;
            }
            return false;
        }

        private void CreateBeam(Transform target, IDamageable damageable)
        {
            ActiveBeam newBeam = new ActiveBeam
            {
                target = target,
                damageable = damageable,
                currentDamage = GetFinalDamage(),
                rampUpTimer = 0f,
                damageTickTimer = 0f,
                line = GetLineRendererFromPool()
            };

            _activeBeams.Add(newBeam);
        }

        private void DestroyBeam(ActiveBeam beam)
        {
            if (beam.line != null)
            {
                ReturnLineRendererToPool(beam.line);
            }
        }

        private LineRenderer GetLineRendererFromPool()
        {
            while (_lineRendererPool.Count > 0)
            {
                LineRenderer lr = _lineRendererPool.Dequeue();
                if (lr != null)
                {
                    lr.gameObject.SetActive(true);
                    return lr;
                }
            }

            GameObject lineObj = new GameObject("TeslaBeam_Pooled");
            lineObj.transform.SetParent(transform);
            LineRenderer newLr = lineObj.AddComponent<LineRenderer>();

            newLr.positionCount = 2;
            newLr.startWidth = beamWidth;
            newLr.endWidth = beamWidth;

            if (beamMaterial != null)
            {
                newLr.sharedMaterial = beamMaterial;
            }
            else
            {
                if (_fallbackBeamMaterial == null)
                {
                    _fallbackBeamMaterial = new Material(Shader.Find("Sprites/Default"));
                }
                newLr.sharedMaterial = _fallbackBeamMaterial;
            }

            newLr.startColor = Color.cyan;
            newLr.endColor = Color.blue;

            return newLr;
        }

        private void ReturnLineRendererToPool(LineRenderer lr)
        {
            if (lr == null) return;
            lr.gameObject.SetActive(false);
            _lineRendererPool.Enqueue(lr);
        }

        public override void ResetWeaponStats()
        {
            base.ResetWeaponStats();
            CurrentMaxDamageCap = 200f;
            CurrentRampUpInterval = 1f;
        }
    }
}