using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class MushroomEntityRefactored : MonoBehaviour, IPausable
    {
        [Header("Efectos Visuales")]
        public Transform sporeVisualArea;
        [Tooltip("Radio mínimo de respaldo por si se instancia con radio 0")]
        public float fallbackRadius = 4.5f;

        private float _damage;
        private float _radius = 4.5f;
        private float _baseRadius = 4.5f;
        private float _duration = 5f;
        private WeaponData.ImpactEffects _effects;
        private float _healAmount;
        private int _weaponID;
        private DamageType _damageType;
        private LayerMask _enemyMask;

        private Vector3 _initialVisualScale = Vector3.one;
        private bool _visualScaleCached;

        private float _tickTimer;
        private float _lifeTimer;
        private bool _isReturned;
        private bool _isPaused;

        private static readonly Collider[] HitBuffer = new Collider[64];
        private static readonly HashSet<IDamageable> DamagedTargets = new HashSet<IDamageable>();

        private static float _nextAllowedPlayerHealTime;
        private static Transform _cachedPlayerTransform;
        private static IHealable _cachedPlayerHealable;
        private static PlayerHealthRefactored _cachedPlayerHealth;

        private void Awake()
        {
            CacheInitialVisualScale();
        }

        private void CacheInitialVisualScale()
        {
            if (_visualScaleCached) return;
            _visualScaleCached = true;

            if (sporeVisualArea != null)
            {
                _initialVisualScale = sporeVisualArea.localScale;
                if (_initialVisualScale.sqrMagnitude < 0.001f)
                {
                    _initialVisualScale = Vector3.one;
                }
            }

            SphereCollider childSphere = GetComponentInChildren<SphereCollider>();
            if (childSphere != null)
            {
                float maxLossy = Mathf.Max(Mathf.Abs(childSphere.transform.lossyScale.x), Mathf.Abs(childSphere.transform.lossyScale.z));
                float detectedWorldRadius = childSphere.radius * maxLossy;
                if (detectedWorldRadius > 0.5f)
                {
                    fallbackRadius = detectedWorldRadius;
                }
            }
        }

        private void OnEnable()
        {
            CacheInitialVisualScale();

            _tickTimer = 0f;
            _lifeTimer = 0f;
            _isReturned = false;
            _isPaused = PauseManager.IsPaused;
            PauseManager.Register(this);

            if (Time.time < _nextAllowedPlayerHealTime)
            {
                _nextAllowedPlayerHealTime = 0f;
            }
        }

        private void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        public void Setup(float dmg, float sporeRadius, float duration, WeaponData.ImpactEffects weaponEffects, float heal, int wID, DamageType dType, LayerMask mask)
        {
            CacheInitialVisualScale();

            _damage = dmg;
            _baseRadius = fallbackRadius > 0.5f ? fallbackRadius : 4.5f;
            _radius = sporeRadius > 0.5f ? Mathf.Max(sporeRadius, _baseRadius) : _baseRadius;
            _duration = duration > 0.1f ? duration : 5f;
            _effects = weaponEffects;
            _healAmount = heal;
            _weaponID = wID;
            _damageType = dType;
            _enemyMask = mask;

            _tickTimer = 0.85f;
            _lifeTimer = 0f;
            _isReturned = false;

            if (sporeVisualArea != null)
            {
                float scaleFactor = Mathf.Clamp(_radius / _baseRadius, 1f, 3f);
                sporeVisualArea.localScale = _initialVisualScale * scaleFactor;
            }
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

            _tickTimer += dt;
            if (_tickTimer >= 1f)
            {
                _tickTimer -= 1f;
                SporeTick();
            }
        }

        private void SporeTick()
        {
            DamagedTargets.Clear();

            int maskToUse = _enemyMask.value != 0 ? _enemyMask.value : Physics.AllLayers;
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _radius, HitBuffer, maskToUse, QueryTriggerInteraction.Collide);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = HitBuffer[i];
                if (hit == null || hit.CompareTag("Player")) continue;

                if (!DamageableRegistry.TryGet(hit, out IDamageable enemy, out Transform root))
                {
                    enemy = hit.GetComponentInParent<IDamageable>();
                    root = enemy is Component comp ? comp.transform : hit.transform;
                }

                if (root != null && root.CompareTag("Player")) continue;

                if (enemy != null && DamagedTargets.Add(enemy))
                {
                    bool isCrit = Random.Range(0f, 100f) <= _effects.critChance;
                    enemy.TakeDamage(_damage, isCrit, _damageType);
                    enemy.ApplyEffects(_effects, _weaponID);
                }
            }

            if (_healAmount > 0f && Time.time >= _nextAllowedPlayerHealTime)
            {
                TryHealPlayerInRadius();
            }
        }

        private void TryHealPlayerInRadius()
        {
            Transform playerTransform = GameManagerRefactored.Instance != null ? GameManagerRefactored.Instance.Player : null;

            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) playerTransform = playerObj.transform;
            }

            if (playerTransform == null) return;

            Vector3 diff = playerTransform.position - transform.position;
            diff.y = 0f;
            if (diff.sqrMagnitude > _radius * _radius) return;

            if (_cachedPlayerTransform != playerTransform || (_cachedPlayerHealable == null && _cachedPlayerHealth == null))
            {
                _cachedPlayerTransform = playerTransform;
                if (!playerTransform.TryGetComponent(out _cachedPlayerHealable))
                {
                    _cachedPlayerHealable = playerTransform.GetComponentInChildren<IHealable>();
                }
                if (!playerTransform.TryGetComponent(out _cachedPlayerHealth))
                {
                    _cachedPlayerHealth = playerTransform.GetComponentInChildren<PlayerHealthRefactored>();
                }
            }

            if (_cachedPlayerHealable != null)
            {
                _nextAllowedPlayerHealTime = Time.time + 0.95f;
                _cachedPlayerHealable.Heal(_healAmount);
            }
            else if (_cachedPlayerHealth != null)
            {
                _nextAllowedPlayerHealTime = Time.time + 0.95f;
                _cachedPlayerHealth.Heal(_healAmount);
            }
        }

        private void ReturnToPool()
        {
            if (_isReturned) return;
            _isReturned = true;

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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _radius > 0.5f ? _radius : fallbackRadius);
        }
    }
}