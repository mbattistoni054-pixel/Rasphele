using UnityEngine;
using System.Collections;

namespace PatronesAplicados.RealImplementation
{
    public class MeteorProjectileRefactored : MonoBehaviour
    {
        [Header("Configuracin")]
        public float fallSpeed = 25f;
        public LayerMask groundMask;
        public GameObject explosionVisualPrefab;

        private float damage;
        private WeaponData.ImpactEffects effects;
        private float extraExplosionRadius;
        private int weaponID;
        private DamageType damageType;
        private LayerMask enemyMask;
        private Vector3 fallDirection;
        
        private Vector3 originalScale;
        private Color originalColor;
        private Coroutine lifeTimerCoroutine;

        private void Awake()
        {
            originalScale = transform.localScale;
            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null) originalColor = rend.material.color;
        }

        public void Setup(float dmg, WeaponData.ImpactEffects fx, float expRadius, int wID, DamageType dType, LayerMask eMask, Vector3 direction, bool isScatter, bool isComet)
        {
            damage = dmg;
            effects = fx;
            extraExplosionRadius = expRadius;
            weaponID = wID;
            damageType = dType;
            enemyMask = eMask;
            fallDirection = direction;

            // Resetear escala y color antes de aplicar modificadores
            transform.localScale = originalScale;
            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = originalColor;

            if (isScatter) transform.localScale *= 0.5f;
            if (isComet && rend != null) rend.material.color = Color.cyan;

            // ! PATRN POOL: Seguridad de tiempo de vida (por si cae al vaco)
            if (lifeTimerCoroutine != null) StopCoroutine(lifeTimerCoroutine);
            lifeTimerCoroutine = StartCoroutine(ReturnToPoolAfterTime(10f));
        }

        private IEnumerator ReturnToPoolAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (lifeTimerCoroutine != null) StopCoroutine(lifeTimerCoroutine);
            
            if (ProjectilePoolManager.Instance != null)
                ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
            else
                Destroy(gameObject);
        }

        void Update()
        {
            float moveDistance = fallSpeed * Time.deltaTime;

            if (Physics.Raycast(transform.position, fallDirection, out RaycastHit hit, moveDistance, groundMask))
            {
                transform.position = hit.point;
                ExplodeOnGround();
                return;
            }

            transform.position += fallDirection * moveDistance;

            if (fallDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(fallDirection);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & enemyMask) != 0 || other.CompareTag("Enemy"))
            {
                IDamageable enemy = other.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    bool isCrit = Random.Range(0f, 100f) <= effects.critChance;
                    enemy.TakeDamage(damage, isCrit, damageType);
                    enemy.ApplyEffects(effects, weaponID);
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
            float finalExplosionRadius = transform.localScale.x + extraExplosionRadius;
            float explosionDamage = damage / 2f;

            if (explosionVisualPrefab != null)
            {
                // El visual tambin podra poolerarse
                GameObject visualObj = Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);
                float visualScale = finalExplosionRadius * 2f;
                visualObj.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
                Destroy(visualObj, 2f); // <-- CORRECCIN: Evita la fuga de memoria
            }

            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, finalExplosionRadius, enemyMask);
            foreach (Collider hit in hitEnemies)
            {
                IDamageable enemy = hit.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    bool isCrit = Random.Range(0f, 100f) <= effects.critChance;
                    enemy.TakeDamage(explosionDamage, isCrit, damageType);
                    enemy.ApplyEffects(effects, weaponID);
                }
            }

            ReturnToPool();
        }
    }
}
