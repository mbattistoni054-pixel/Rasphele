using UnityEngine;

public class EnemyRange : EnemyBase
{
    private float lastAttackTime;
    bool onAttack;

    [SerializeField] Animator animator;
    [SerializeField] Transform firePoint;

    private bool hasMoveParam = false;
    private float pathTimer = 0f;

    protected override void Start()
    {
        base.Start();
        if (animator == null) animator = GetComponent<Animator>();

        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == "Move") hasMoveParam = true;
            }
        }

        if (agent != null)
        {
            agent.stoppingDistance = data.RangeAttack;
            agent.acceleration = 60f;      // Frena y arranca rápido sin patinar
            agent.angularSpeed = 600f;     // Gira muy rápido
        }
    }

    protected override void Update()
    {
        base.Update();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        if (player != null && !isStunned && agent != null)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            // LÓGICA DE MOVIMIENTO 
            if (!onAttack && distance > data.RangeAttack)
            {
                agent.isStopped = false;

                if (Time.time >= pathTimer)
                {
                    agent.SetDestination(player.position);
                    pathTimer = Time.time + 0.2f;
                }

                if (animator != null && hasMoveParam) animator.SetBool("Move", true);
            }
            else
            {
                agent.isStopped = true;

                if (animator != null && hasMoveParam) animator.SetBool("Move", false);
            }

            // Forzamos a que mire al jugador suavemente si está en rango o atacando
            if (distance <= data.RangeAttack || onAttack)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0; // Evita que se incline hacia arriba/abajo

                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
                }
            }

            // LÓGICA DE ATAQUE 
            if (distance <= data.RangeAttack && Time.time >= lastAttackTime + data.AttackCooldown)
            {
                onAttack = true;
                lastAttackTime = Time.time;
                if (animator != null) animator.SetTrigger("Shoot");

                Invoke(nameof(AttackEnd), 1f);
            }
        }
    }

    public void Shoot()
    {
        if (data.Bullet == null || firePoint == null) return;
        GameObject obj = Instantiate(data.Bullet, firePoint.position, firePoint.rotation);
        BulletEnemy bulletEnemy = obj.GetComponent<BulletEnemy>();
        if (bulletEnemy != null) bulletEnemy.damageMultiplier = damageMultiplier;
        if (bulletEnemy != null) bulletEnemy.data = data;
    }

    public void AttackEnd()
    {
        onAttack = false;
    }
}