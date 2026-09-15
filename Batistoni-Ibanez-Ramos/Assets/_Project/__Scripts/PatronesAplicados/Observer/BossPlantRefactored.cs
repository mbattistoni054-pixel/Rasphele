using UnityEngine;
using System.Collections;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public class BossPlantRefactored : EnemyBaseRefactored
    {
        [Header("Ajustes de Spawn")]
        public float spawnHeightOffset = 0f;

        [Header("Estadisticas del Jefe")]
        public float attackDamage = 30f;
        public float attackCooldown = 3f;

        [Header("Ataque A (70%): Rafaga de Orbes")]
        public GameObject orbPrefab;
        public Transform firePoint;
        public int orbsPerBurst = 6;
        public float timeBetweenOrbs = 0.15f;

        [Header("Ataque B (30%): Trampa de Raices")]
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

        protected override void Start()
        {
            base.Start();

            currentHealth = maxHealth;

            if (animator == null) animator = GetComponent<Animator>();

            if (agent != null)
            {
                agent.isStopped = true;
                agent.speed = 0f;
            }

            if (spawnHeightOffset != 0f)
            {
                transform.position += Vector3.up * spawnHeightOffset;
            }

            lastHealth = currentHealth;
            
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("BossSpawned", "Planta Mutante", currentHealth, maxHealth);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (currentHealth != lastHealth)
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

            if (player == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
                else return;
            }

            if (!isStunned && !isAttacking)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0;

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

            if (player != null && Vector3.Distance(transform.position, player.position) <= meleeRange + 1f)
            {
                var hpRefactored = player.GetComponent<PlayerHealthRefactored>();
                if (hpRefactored != null) hpRefactored.TakeDamage(meleeDamage);
                else
                {
                    var hp = player.GetComponent<PlayerHealth>();
                    if (hp != null) hp.TakeDamage(meleeDamage);
                }

                var pcRefactored = player.GetComponent<PlayerControllerRefactored>();
                if (pcRefactored != null)
                {
                    Vector3 pushDir = (player.position - transform.position).normalized;
                    pushDir.y = 0;
                    Vector3 finalKnockback = (pushDir * knockbackHorizontal) + (Vector3.up * knockbackUpward);
                    pcRefactored.ApplyKnockback(finalKnockback);
                }
                else
                {
                    var pc = player.GetComponent<CopiaPlayerController2>();
                    if (pc != null)
                    {
                        Vector3 pushDir = (player.position - transform.position).normalized;
                        pushDir.y = 0;
                        Vector3 finalKnockback = (pushDir * knockbackHorizontal) + (Vector3.up * knockbackUpward);
                        pc.ApplyKnockback(finalKnockback);
                    }
                }
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
            if (animator != null) animator.SetTrigger("Shoot");

            int orbsToShoot = isPhase2 ? orbsPerBurstPhase2 : orbsPerBurst;

            for (int i = 0; i < orbsToShoot; i++)
            {
                if (player == null) break;

                Vector3 aimDirection = (player.position - firePoint.position).normalized;
                GameObject orb;
                if (ProjectilePoolManager.Instance != null)
                    orb = ProjectilePoolManager.Instance.GetProjectile(orbPrefab, firePoint.position, Quaternion.LookRotation(aimDirection));
                else
                    orb = Instantiate(orbPrefab, firePoint.position, Quaternion.LookRotation(aimDirection));

                NEWBossOrb orbScript = orb.GetComponent<NEWBossOrb>();
                BossOrb oldOrbScript = orb.GetComponent<BossOrb>();
                if (orbScript != null)
                {
                    orbScript.Setup(attackDamage);
                }
                else if (oldOrbScript != null)
                {
                    oldOrbScript.Setup(attackDamage);
                }


                yield return new WaitForSeconds(timeBetweenOrbs);
            }
        }

        private IEnumerator AttackB_Roots_Routine()
        {
            int rootsToSpawn = isPhase2 ? rootsPhase2 : 1;

            for (int i = 0; i < rootsToSpawn; i++)
            {
                if (animator != null) animator.SetTrigger("Cast");

                if (rootTrapPrefab != null && player != null)
                {
                    GameObject trap;
                    if (ProjectilePoolManager.Instance != null)
                        trap = ProjectilePoolManager.Instance.GetProjectile(rootTrapPrefab, player.position, Quaternion.identity);
                    else
                        trap = Instantiate(rootTrapPrefab, player.position, Quaternion.identity);

                    NEWBossRootTrap trapScript = trap.GetComponent<NEWBossRootTrap>();
                    BossRootTrap oldTrapScript = trap.GetComponent<BossRootTrap>();

                    if (trapScript != null)
                    {
                        trapScript.Setup(player, attackDamage);
                    }
                    else if (oldTrapScript != null)
                    {
                        oldTrapScript.Setup(player, attackDamage);
                    }

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





