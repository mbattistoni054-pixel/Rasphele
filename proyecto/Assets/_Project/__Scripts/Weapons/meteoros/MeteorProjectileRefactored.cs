using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class MeteorProjectileRefactored : MonoBehaviour, IPausable
    {
        [Header("Configuración")]
        public float fallSpeed = 25f;
        public float maxLifeTime = 10f;
        public LayerMask groundMask;
        public GameObject explosionVisualPrefab;

        private float _damage;
        private WeaponData.ImpactEffects _effects;
        private float _extraExplosionRadius;
        private int _weaponID;
        private DamageType _damageType;
        private LayerMask _enemyMask;
        private Vector3 _fallDirection;

        private Vector3 _originalScale;
        private Renderer _cachedRenderer;
        private MaterialPropertyBlock _propBlock;
        private Color _originalColor = Color.white;

        private float _lifeTimer;
        private bool _isReturned;
        private bool _isPaused;

        private static readonly Collider[] HitBuffer = new Collider[64];
        private static readonly HashSet<IDamageable> DamagedTargets = new HashSet<IDamageable>();
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _originalScale = transform.localScale;
            _cachedRenderer = GetComponentInChildren<Renderer>();
            _propBlock = new MaterialPropertyBlock();

            if (_cachedRenderer != null && _cachedRenderer.sharedMaterial != null)
            {
                if (_cachedRenderer.sharedMaterial.HasProperty(BaseColorId))
                    _originalColor = _cachedRenderer.sharedMaterial.GetColor(BaseColorId);
                else if (_cachedRenderer.sharedMaterial.HasProperty(ColorId))
                    _originalColor = _cachedRenderer.sharedMaterial.GetColor(ColorId);
            }
        }

        private void OnEnable()
        {
            _lifeTimer = 0f;
            _isReturned = false;
            _isPaused = PauseManager.IsPaused;
            PauseManager.Register(this);
        }

        private void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        public void Setup(float dmg, WeaponData.ImpactEffects fx, float expRadius, int wID, DamageType dType, LayerMask eMask, Vector3 direction, bool isScatter, bool isComet)
        {
            _damage = dmg;
            _effects = fx;
            _extraExplosionRadius = expRadius;
            _weaponID = wID;
            _damageType = dType;
            _enemyMask = eMask;
            _fallDirection = direction != Vector3.zero ? direction.normalized : Vector3.down;

            _lifeTimer = 0f;
            _isReturned = false;

            transform.localScale = isScatter ? _originalScale * 0.5f : _originalScale;
            ApplyMeteorColor(isComet ? Color.cyan : _originalColor);
        }

        private void ApplyMeteorColor(Color targetColor)
        {
            if (_cachedRenderer == null) return;
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            _cachedRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(BaseColorId, targetColor);
            _propBlock.SetColor(ColorId, targetColor);
            _cachedRenderer.SetPropertyBlock(_propBlock);
        }

        private void Update()
        {
            if (_isReturned || _isPaused) return;

            float dt = Time.deltaTime;
            _lifeTimer += dt;
            if (_lifeTimer >= maxLifeTime)
            {
                ReturnToPool();
                return;
            }

            float moveDistance = fallSpeed * dt;

            if (Physics.Raycast(transform.position, _fallDirection, out RaycastHit hit, moveDistance, groundMask))
            {
                transform.position = hit.point;
                ExplodeOnGround();
                return;
            }

            transform.position += _fallDirection * moveDistance;

            if (_fallDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(_fallDirection);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isReturned || _isPaused) return;

            if (((1 << other.gameObject.layer) & _enemyMask) != 0 || other.CompareTag("Enemy"))
            {
                if (DamageableRegistry.TryGet(other, out IDamageable enemy, out _))
                {
                    bool isCrit = Random.Range(0f, 100f) <= _effects.critChance;
                    enemy.TakeDamage(_damage, isCrit, _damageType);
                    enemy.ApplyEffects(_effects, _weaponID);
                }

                ReturnToPool();
            }
            else if (((1 << other.gameObject.layer) & groundMask) != 0 || other.CompareTag("Ground"))
            {
                ExplodeOnGround();
            }
        }

        private void ExplodeOnGround()
        {
            if (_isReturned) return;

            float finalExplosionRadius = transform.localScale.x + _extraExplosionRadius;
            float explosionDamage = _damage * 0.5f;

            if (explosionVisualPrefab != null)
            {
                GameObject visualObj = VfxPoolManager.Instance != null ? VfxPoolManager.Instance.GetVfx(explosionVisualPrefab, transform.position, Quaternion.identity, 2f) : Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);

                if (visualObj != null)
                {
                    float visualScale = finalExplosionRadius * 2f;
                    visualObj.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
                }
            }

            DamagedTargets.Clear();
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, finalExplosionRadius, HitBuffer, _enemyMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = HitBuffer[i];
                if (hit == null) continue;

                if (DamageableRegistry.TryGet(hit, out IDamageable enemy, out _) && DamagedTargets.Add(enemy))
                {
                    bool isCrit = Random.Range(0f, 100f) <= _effects.critChance;
                    enemy.TakeDamage(explosionDamage, isCrit, _damageType);
                    enemy.ApplyEffects(_effects, _weaponID);
                }
            }

            ReturnToPool();
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
    }
}