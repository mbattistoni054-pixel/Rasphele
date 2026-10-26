using System.Collections;
using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public class BossPlantRefactored : EnemyBaseRefactored
    {
        [Header("Ajustes de Spawn")]
        public string bossDisplayName = "Planta Mutante";
        public float spawnHeightOffset = 0f;

        [Header("Estadísticas del Jefe (Fallback si no hay EnemyData)")]
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
        private bool _isPhase2;

        [Header("Ataque C (Defensivo): Golpe Cuerpo a Cuerpo")]
        public float meleeRange = 5f;
        public float meleeDamage = 40f;
        public float knockbackHorizontal = 25f;
        public float knockbackUpward = 5f;
        public float meleeHitDelay = 0.5f;

        [Header("Animaciones (Opcional)")]
        public Animator animator;

        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int ShootHash = Animator.StringToHash("Shoot");
        private static readonly int CastHash = Animator.StringToHash("Cast");

        protected override bool UsesNavigation => false;

        private float _attackTimer;
        private bool _isAttacking;
        private bool _heightOffsetApplied;

        protected override void Awake()
        {
            base.Awake();
            SyncConfigFromEnemyData();
        }

        private void SyncConfigFromEnemyData()
        {
            if (data == null) return;

            if (data.AttackDamage > 0f) attackDamage = data.AttackDamage;
            if (data.AttackCooldown > 0f) attackCooldown = data.AttackCooldown;
            if (data.OrbPrefab != null) orbPrefab = data.OrbPrefab;
            if (data.OrbsPerBurst > 0) orbsPerBurst = data.OrbsPerBurst;
            if (data.TimeBetweenOrbs > 0f) timeBetweenOrbs = data.TimeBetweenOrbs;
            if (data.RootTrapPrefab != null) rootTrapPrefab = data.RootTrapPrefab;
            if (data.OrbsPerBurstPhase2 > 0) orbsPerBurstPhase2 = data.OrbsPerBurstPhase2;
            if (data.RootsPhase2 > 0) rootsPhase2 = data.RootsPhase2;
            if (data.DelayBetweenRootsPhase2 > 0f) delayBetweenRootsPhase2 = data.DelayBetweenRootsPhase2;
            if (data.MeleeRange > 0f) meleeRange = data.MeleeRange;
            if (data.MeleeDamage > 0f) meleeDamage = data.MeleeDamage;
            if (data.BossKnockbackHorizontal > 0f) knockbackHorizontal = data.BossKnockbackHorizontal;
            if (data.BossKnockbackUpward > 0f) knockbackUpward = data.BossKnockbackUpward;
            if (data.MeleeHitDelay > 0f) meleeHitDelay = data.MeleeHitDelay;
        }

        protected override void Start()
        {
            base.Start();

            if (animator == null) animator = GetComponent<Animator>();

            if (!_heightOffsetApplied && spawnHeightOffset != 0f)
            {
                transform.position += Vector3.up * spawnHeightOffset;
                _heightOffsetApplied = true;
            }

            NotifyBossSpawned();
        }

        public override void ResetStats()
        {
            base.ResetStats();
            SyncConfigFromEnemyData();

            _isPhase2 = false;
            _isAttacking = false;
            _attackTimer = 0f;

            NotifyBossSpawned();
        }

        private void NotifyBossSpawned()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.BossSpawned, bossDisplayName, currentHealth, maxHealth);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (isDead || isPaused || player == null || isStunned || _isAttacking) return;

            RotateTowardsPlayer(Time.deltaTime, 3f);

            _attackTimer += Time.deltaTime;
            if (_attackTimer >= attackCooldown)
            {
                _attackTimer = 0f;

                float sqrDistance = (player.position - transform.position).sqrMagnitude;
                if (sqrDistance <= meleeRange * meleeRange)
                {
                    StartCoroutine(MeleeAttackRoutine());
                }
                else
                {
                    StartCoroutine(DecideAttackRoutine());
                }
            }
        }

        public override void TakeDamage(float amount, bool isCrit, DamageType type, bool isBleed = false)
        {
            if (isDead) return;

            if (_isPhase2)
            {
                amount *= 0.5f;
            }

            base.TakeDamage(amount, isCrit, type, isBleed);

            if (!isDead && !_isPhase2 && currentHealth <= maxHealth * 0.5f)
            {
                _isPhase2 = true;
            }

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.BossHealthChanged, currentHealth, maxHealth);
            }
        }

        public override void OnPause()
        {
            base.OnPause();
            if (animator != null) animator.speed = 0f;
        }

        public override void OnResume()
        {
            base.OnResume();
            if (animator != null) animator.speed = 1f;
        }

        private IEnumerator WaitForSecondsPausable(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (!isPaused && !PauseManager.IsPaused)
                {
                    elapsed += Time.deltaTime;
                }
                yield return null;
            }
        }

        private IEnumerator MeleeAttackRoutine()
        {
            _isAttacking = true;

            if (animator != null) animator.SetTrigger(AttackHash);

            yield return WaitForSecondsPausable(meleeHitDelay);

            float hitRadius = meleeRange + 1f;
            if (!isDead && !isStunned && player != null &&
                (player.position - transform.position).sqrMagnitude <= hitRadius * hitRadius)
            {
                float finalMeleeDmg = meleeDamage * damageMultiplier;

                if (player.TryGetComponent(out PlayerHealthRefactored hpRefactored))
                {
                    hpRefactored.TakeDamage(finalMeleeDmg);
                }

                Vector3 pushDir = player.position - transform.position;
                pushDir.y = 0f;
                pushDir.Normalize();
                Vector3 finalKnockback = (pushDir * knockbackHorizontal) + (Vector3.up * knockbackUpward);

                if (player.TryGetComponent(out PlayerControllerRefactored pcRefactored))
                {
                    pcRefactored.ApplyKnockback(finalKnockback);
                }
            }

            yield return WaitForSecondsPausable(1f);
            _isAttacking = false;
        }

        private IEnumerator DecideAttackRoutine()
        {
            _isAttacking = true;
            float randomRoll = Random.Range(0f, 100f);

            if (randomRoll <= 70f)
            {
                yield return StartCoroutine(AttackA_Burst());
            }
            else
            {
                yield return StartCoroutine(AttackB_Roots_Routine());
            }

            _isAttacking = false;
        }

        private IEnumerator AttackA_Burst()
        {
            if (orbPrefab == null) yield break;
            if (animator != null) animator.SetTrigger(ShootHash);

            int orbsToShoot = _isPhase2 ? orbsPerBurstPhase2 : orbsPerBurst;
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
                    {
                        orbScript.Setup(finalDamage);
                    }
                }

                yield return WaitForSecondsPausable(timeBetweenOrbs);
            }
        }

        private IEnumerator AttackB_Roots_Routine()
        {
            if (rootTrapPrefab == null) yield break;

            int rootsToSpawn = _isPhase2 ? rootsPhase2 : 1;
            float finalDamage = attackDamage * damageMultiplier;

            for (int i = 0; i < rootsToSpawn; i++)
            {
                if (isDead || isStunned || player == null) break;

                if (animator != null) animator.SetTrigger(CastHash);

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
                    {
                        trapScript.Setup(player, finalDamage);
                    }
                }

                if (i < rootsToSpawn - 1)
                {
                    yield return WaitForSecondsPausable(delayBetweenRootsPhase2);
                }
            }

            yield return WaitForSecondsPausable(1.5f);
        }

        protected override void Die()
        {
            if (isDead) return;
            _isAttacking = false;

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.BossDefeated);
                EventManager.Instance.TriggerEvent(GameEventKeys.LevelComplete);
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