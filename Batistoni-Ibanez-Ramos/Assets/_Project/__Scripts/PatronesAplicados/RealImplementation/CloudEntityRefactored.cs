using UnityEngine;
using System.Collections;

namespace PatronesAplicados.RealImplementation
{
    public class CloudEntityRefactored : MonoBehaviour
    {
        private float damage;
        private float radius;
        private WeaponData.ImpactEffects effects;
        private float lightningChance;
        private int weaponID;
        private DamageType damageType;
        private LayerMask enemyMask;

        [Header("Telegrafo (Crculo Visual)")]
        public Transform aoeVisual;

        [Header("Efecto del Rayo")] 
        public GameObject lightningVisualPrefab;

        private float rainTimer = 0f;
        private float lightningTimer = 0f;
        private Coroutine lifeTimerCoroutine;

        public void Setup(float weaponDamage, float cloudRadius, float duration, WeaponData.ImpactEffects weaponEffects, float electricChance, int wID, DamageType dType, LayerMask mask)
        {
            damage = weaponDamage;
            radius = cloudRadius;
            effects = weaponEffects;
            lightningChance = electricChance;
            weaponID = wID;
            damageType = dType;
            enemyMask = mask;
            
            rainTimer = 0f;
            lightningTimer = 0f;

            if (aoeVisual != null)
            {
                aoeVisual.gameObject.SetActive(true);
                aoeVisual.localScale = new Vector3(radius * 2f, 0.05f, radius * 2f);
            }

            UpdateAoEVisual();

            if (lifeTimerCoroutine != null) StopCoroutine(lifeTimerCoroutine);
            lifeTimerCoroutine = StartCoroutine(ReturnToPoolAfterTime(duration));
        }

        private IEnumerator ReturnToPoolAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            
            if (aoeVisual != null) aoeVisual.gameObject.SetActive(false);
            
            if (ProjectilePoolManager.Instance != null)
                ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
            else
                Destroy(gameObject);
        }

        void Update()
        {
            rainTimer += Time.deltaTime;
            if (rainTimer >= 0.2f)
            {
                rainTimer -= 0.2f;
                RainDamage();
            }

            if (lightningChance > 0)
            {
                lightningTimer += Time.deltaTime;
                if (lightningTimer >= 1f)
                {
                    lightningTimer -= 1f;
                    if (Random.Range(0f, 100f) <= lightningChance)
                    {
                        CastLightning();
                    }
                }
            }
        }

        private void UpdateAoEVisual()
        {
            if (aoeVisual == null) return;

            int mascaraSueloFija = LayerMask.GetMask("Ground");
            Vector3 rayOrigin = transform.position + Vector3.up * 2f;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, mascaraSueloFija, QueryTriggerInteraction.Ignore))
            {
                aoeVisual.position = hit.point + (Vector3.up * 0.05f);
                aoeVisual.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                Vector3 fallbackPos = new Vector3(transform.position.x, 0.05f, transform.position.z);
                aoeVisual.position = fallbackPos;
                aoeVisual.rotation = Quaternion.identity;
            }
        }

        private void RainDamage()
        {
            Vector3 groundPosition = (aoeVisual != null) ? aoeVisual.position : transform.position - (Vector3.up * 4f);

            Collider[] hits = Physics.OverlapSphere(groundPosition, radius, enemyMask);
            foreach (Collider hit in hits)
            {
                IDamageable enemy = hit.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    bool isCrit = Random.Range(0f, 100f) <= effects.critChance;
                    enemy.TakeDamage(damage, isCrit, damageType);
                    enemy.ApplyEffects(effects, weaponID);
                }
            }
        }

        private void CastLightning()
        {
            Vector3 groundPosition = (aoeVisual != null) ? aoeVisual.position : transform.position - (Vector3.up * 4f);
            float lightningRadius = radius * 1.5f;

            if (lightningVisualPrefab != null)
            {
                GameObject rayoObj = Instantiate(lightningVisualPrefab, groundPosition, Quaternion.identity);
                Destroy(rayoObj, 1f);
            }

            Collider[] hits = Physics.OverlapSphere(groundPosition, lightningRadius, enemyMask);
            foreach (Collider hit in hits)
            {
                IDamageable enemy = hit.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage * 2f, false, DamageType.Electrico);

                    WeaponData.ImpactEffects lightningEffects = new WeaponData.ImpactEffects();
                    lightningEffects.stunChance = 100f;
                    enemy.ApplyEffects(lightningEffects, weaponID + 1000);
                }
            }
        }
    }
}
