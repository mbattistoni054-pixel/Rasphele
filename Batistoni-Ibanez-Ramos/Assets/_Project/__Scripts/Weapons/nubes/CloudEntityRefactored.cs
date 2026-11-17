using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class CloudEntityRefactored : MonoBehaviour, IPausable
    {
        private float _damage;
        private float _radius;
        private float _duration;
        private WeaponData.ImpactEffects _effects;
        private float _lightningChance;
        private int _weaponID;
        private DamageType _damageType;
        private LayerMask _enemyMask;

        [Header("Telégrafo (Círculo Visual)")]
        public Transform aoeVisual;

        [Header("Efecto del Rayo")]
        public GameObject lightningVisualPrefab;

        private float _rainTimer;
        private float _lightningTimer;
        private float _lifeTimer;
        private bool _isReturned;
        private bool _isPaused;

        private static readonly Collider[] HitBuffer = new Collider[64];
        private static readonly HashSet<IDamageable> DamagedTargets = new HashSet<IDamageable>();
        private static int _cachedGroundMask = -1;

        private void OnEnable()
        {
            _rainTimer = 0f;
            _lightningTimer = 0f;
            _lifeTimer = 0f;
            _isReturned = false;
            _isPaused = PauseManager.IsPaused;
            PauseManager.Register(this);
        }

        private void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        public void Setup(float weaponDamage, float cloudRadius, float duration, WeaponData.ImpactEffects weaponEffects, float electricChance, int wID, DamageType dType, LayerMask mask)
        {
            _damage = weaponDamage;
            _radius = cloudRadius;
            _duration = duration;
            _effects = weaponEffects;
            _lightningChance = electricChance;
            _weaponID = wID;
            _damageType = dType;
            _enemyMask = mask;

            _rainTimer = 0f;
            _lightningTimer = 0f;
            _lifeTimer = 0f;
            _isReturned = false;

            if (aoeVisual != null)
            {
                aoeVisual.gameObject.SetActive(true);
                aoeVisual.localScale = new Vector3(_radius * 2f, 0.05f, _radius * 2f);
            }

            UpdateAoEVisual();
        }

        private void Update()
        {
            if (_isReturned || _isPaused) return;

            float dt = Time.deltaTime;

            _lifeTimer += dt;
            if (_lifeTimer >= _duration)
            {
                ReturnToPool();
                return;
            }

            _rainTimer += dt;
            if (_rainTimer >= 0.2f)
            {
                _rainTimer -= 0.2f;
                RainDamage();
            }

            if (_lightningChance > 0f)
            {
                _lightningTimer += dt;
                if (_lightningTimer >= 1f)
                {
                    _lightningTimer -= 1f;
                    if (Random.Range(0f, 100f) <= _lightningChance)
                    {
                        CastLightning();
                    }
                }
            }
        }

        private void UpdateAoEVisual()
        {
            if (aoeVisual == null) return;

            if (_cachedGroundMask == -1)
            {
                _cachedGroundMask = LayerMask.GetMask("Ground");
            }

            Vector3 rayOrigin = transform.position + Vector3.up * 2f;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, _cachedGroundMask, QueryTriggerInteraction.Ignore))
            {
                aoeVisual.position = hit.point + (Vector3.up * 0.05f);
                aoeVisual.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                aoeVisual.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
                aoeVisual.rotation = Quaternion.identity;
            }
        }

        private void RainDamage()
        {
            Vector3 groundPosition = (aoeVisual != null) ? aoeVisual.position : transform.position - (Vector3.up * 4f);

            DamagedTargets.Clear();
            int hitCount = Physics.OverlapSphereNonAlloc(groundPosition, _radius, HitBuffer, _enemyMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = HitBuffer[i];
                if (hit == null) continue;

                if (DamageableRegistry.TryGet(hit, out IDamageable enemy, out _) && DamagedTargets.Add(enemy))
                {
                    bool isCrit = Random.Range(0f, 100f) <= _effects.critChance;
                    enemy.TakeDamage(_damage, isCrit, _damageType);
                    enemy.ApplyEffects(_effects, _weaponID);
                }
            }
        }

        private void CastLightning()
        {
            Vector3 groundPosition = (aoeVisual != null) ? aoeVisual.position : transform.position - (Vector3.up * 4f);
            float lightningRadius = _radius * 1.5f;

            if (lightningVisualPrefab != null)
            {
                if (VfxPoolManager.Instance != null)
                {
                    VfxPoolManager.Instance.GetVfx(lightningVisualPrefab, groundPosition, Quaternion.identity, 1f);
                }
                else
                {
                    GameObject rayoObj = Instantiate(lightningVisualPrefab, groundPosition, Quaternion.identity);
                    Destroy(rayoObj, 1f);
                }
            }

            WeaponData.ImpactEffects lightningEffects = new WeaponData.ImpactEffects
            {
                stunChance = 100f
            };

            DamagedTargets.Clear();
            int hitCount = Physics.OverlapSphereNonAlloc(groundPosition, lightningRadius, HitBuffer, _enemyMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = HitBuffer[i];
                if (hit == null) continue;

                if (DamageableRegistry.TryGet(hit, out IDamageable enemy, out _) && DamagedTargets.Add(enemy))
                {
                    enemy.TakeDamage(_damage * 2f, false, DamageType.Electrico);
                    enemy.ApplyEffects(lightningEffects, _weaponID + 1000);
                }
            }
        }

        private void ReturnToPool()
        {
            if (_isReturned) return;
            _isReturned = true;

            if (aoeVisual != null)
            {
                aoeVisual.gameObject.SetActive(false);
            }

            if (ProjectilePoolManager.Instance != null)
            {
                ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OnPause() => _isPaused = true;
        public void OnResume() => _isPaused = false;
    }
}