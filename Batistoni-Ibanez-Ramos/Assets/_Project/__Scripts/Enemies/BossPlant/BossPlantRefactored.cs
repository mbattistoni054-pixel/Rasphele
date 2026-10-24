using System.Collections;
using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public class BossPlantRefactored : EnemyBaseRefactored
    {
        [Header("Ajustes de Spawn")]
        public float spawnHeightOffset = 0f;

        [Header("Estadísticas del Jefe")]
        public float attackDamage = 30f;
        public float attackCooldown = 3f;

        [Header("Ataque A (70%): Ráfaga de Orbes")]
        public GameObject orbPrefab;
        public Transform firePoint;
        public int orbsPerBurst = 6;
        public float timeBetweenOrbs = 0.15f;

        [Header("Ataque B (30%): Trampa de Raíces")]
        public GameObject rootTrapPrefab;

        [Header("Fase 2 (Menos del 50% Vida)")]
        public int orbsPerBurstPhase2 = 12;
        public int rootsPhase2 = 3;
        public float delayBetweenRootsPhase2 = 1f;
        private bool isPhase2 = false;

        [Header("Ataque C (Defensivo): Golpe Cuerpo a Cuerpo")]
        public float meleeRange = 5f;
        public float meleeDamage = 40f;
        public float knockbackHorizontal = 25f;
        public float knockbackUpward = 5f;
        public float meleeHitDelay = 0.5f;

        [Header("Animaciones (Opcional)")]
        public Animator animator;

        private float attackTimer;
        private bool isAttacking = false;
        private float lastHealth;
        private bool heightOffsetApplied = false;

        protected override void Start()
        {
            base.Start();

            if (data != null && maxHealth <= 0f)
            {
                maxHealth = data.MaxHealth;
            }
            currentHealth = maxHealth;

            if (animator == null) animator = GetComponent<Animator>();

            if (agent != null)
            {
                agent.speed = 0f;
                if (agent.isOnNavMesh) agent.isStopped = true;
            }

            if (!heightOffsetApplied && spawnHeightOffset != 0f)
            {
                transform.position += Vector3.up * spawnHeightOffset;
                heightOffsetApplied = true;
            }

            lastHealth = currentHealth;

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("BossSpawned", "Planta Mutante", currentHealth, maxHealth);
            }
        }

        public override void ResetStats()
        {
            base.ResetStats();

            isPhase2 = false;
            isAttacking = false;
            attackTimer = 0f;
            lastHealth = currentHealth;

            if (agent != null)
            {
                agent.speed = 0f;
                if (agent.isOnNavMesh) agent.isStopped = true;
            }

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("BossSpawned", "Planta Mutante", currentHealth, maxHealth);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (isDead) return;

            if (!Mathf.Approximately(currentHealth, lastHealth))
            {
                lastHealth = currentHealth;

                if (EventManager.Instance != null)
                {
                    EventManager.Instance.TriggerEvent("BossHealthChanged", currentHealth, maxHealth);
                }

                if (!isPhase2 && currentHealth <= maxHealth * 0.5f)
                {
                    isPhase2 = true;
                }
            }

            if (player == null) return;

            if (!isStunned && !isAttacking)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0f;

                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
                }

                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;

                    float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                    if (distanceToPlayer <= meleeRange)
                    {
                        StartCoroutine(MeleeAttackRoutine());
                    }
                    else
                    {
                        StartCoroutine(DecideAttackRoutine());
                    }
                }
            }
        }

        public override void TakeDamage(float amount, bool isCrit, DamageType type, bool isBleed = false)
        {
            if (isPhase2)
            {
                amount *= 0.5f;
            }

            base.TakeDamage(amount, isCrit, type, isBleed);
        }

        private IEnumerator MeleeAttackRoutine()
        {
            isAttacking = true;

            if (animator != null) animator.SetTrigger("Attack");

            yield return new WaitForSeconds(meleeHitDelay);

            if (!isDead && !isStunned && player != null && Vector3.Distance(transform.position, player.position) <= meleeRange + 1f)
            {
                float finalMeleeDmg = meleeDamage * damageMultiplier;

                if (player.TryGetComponent(out PlayerHealthRefactored hpRefactored))
                    hpRefactored.TakeDamage(finalMeleeDmg);

                Vector3 pushDir = (player.position - transform.position).normalized;
                pushDir.y = 0f;
                Vector3 finalKnockback = (pushDir * knockbackHorizontal) + (Vector3.up * knockbackUpward);

                if (player.TryGetComponent(out PlayerControllerRefactored pcRefactored))
                    pcRefactored.ApplyKnockback(finalKnockback);
            }

            yield return new WaitForSeconds(1f);
            isAttacking = false;
        }

        private IEnumerator DecideAttackRoutine()
        {
            isAttacking = true;
            float randomRoll = Random.Range(0f, 100f);

            if (randomRoll <= 70f)
            {
                yield return StartCoroutine(AttackA_Burst());
            }
            else
            {
                yield return StartCoroutine(AttackB_Roots_Routine());
            }

            isAttacking = false;
        }

        private IEnumerator AttackA_Burst()
        {
            if (orbPrefab == null) yield break;
            if (animator != null) animator.SetTrigger("Shoot");

            int orbsToShoot = isPhase2 ? orbsPerBurstPhase2 : orbsPerBurst;
            float finalDamage = attackDamage * damageMultiplier;

            for (int i = 0; i < orbsToShoot; i++)
            {
                if (isDead || isStunned || player == null) break;

                Vector3 spawnOrigin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1.5f;
                Vector3 aimDirection = ((player.position + Vector3.up * 1f) - spawnOrigin).normalized;
                if (aimDirection == Vector3.zero) aimDirection = transform.forward;
                Quaternion rot = Quaternion.LookRotation(aimDirection);

                if (ProjectilePoolManager.Instance != null)
                {
                    NEWBossOrb orbScript = ProjectilePoolManager.Instance.GetProjectile<NEWBossOrb>(orbPrefab, spawnOrigin, rot);
                    if (orbScript != null)
                    {
                        orbScript.Setup(finalDamage);
                    }
                }
                else
                {
                    GameObject orbObj = Instantiate(orbPrefab, spawnOrigin, rot);
                    if (orbObj.TryGetComponent(out NEWBossOrb orbScript))
                        orbScript.Setup(finalDamage);
                }

                yield return new WaitForSeconds(timeBetweenOrbs);
            }
        }

        private IEnumerator AttackB_Roots_Routine()
        {
            if (rootTrapPrefab == null) yield break;

            int rootsToSpawn = isPhase2 ? rootsPhase2 : 1;
            float finalDamage = attackDamage * damageMultiplier;

            for (int i = 0; i < rootsToSpawn; i++)
            {
                if (isDead || isStunned || player == null) break;

                if (animator != null) animator.SetTrigger("Cast");

                if (ProjectilePoolManager.Instance != null)
                {
                    NEWBossRootTrap trapScript = ProjectilePoolManager.Instance.GetProjectile<NEWBossRootTrap>(rootTrapPrefab, player.position, Quaternion.identity);
                    if (trapScript != null)
                    {
                        trapScript.Setup(player, finalDamage);
                    }
                }
                else
                {
                    GameObject trapObj = Instantiate(rootTrapPrefab, player.position, Quaternion.identity);
                    if (trapObj.TryGetComponent(out NEWBossRootTrap trapScript))
                        trapScript.Setup(player, finalDamage);
                }

                if (i < rootsToSpawn - 1)
                {
                    yield return new WaitForSeconds(delayBetweenRootsPhase2);
                }
            }

            yield return new WaitForSeconds(1.5f);
        }

        protected override void Die()
        {
            if (isDead) return;
            isAttacking = false;

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("BossDefeated");
                EventManager.Instance.TriggerEvent("LevelComplete");
            }

            base.Die();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, meleeRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, meleeRange);
        }
    }
}