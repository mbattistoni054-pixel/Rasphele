using UnityEngine;

public class CopiaMeleeEnemy : EnemyBase
{
    [Header("Estadísticas de Ataque")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f;

    private float lastAttackTime;
    private float lastFrameTime;

    bool onAttack;

    [SerializeField] Animator animator;

    private bool hasMoveParam = false;
    private float pathTimer = 0f;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == "Move") hasMoveParam = true;
            }
        }

        if (agent != null)
        {
            agent.stoppingDistance = 1.5f; // Que frene exactamente en el rango de ataque
            agent.acceleration = 60f;      // Mucha aceleración para que frene y arranque de golpe (sin patinar)
            agent.angularSpeed = 600f;     // Que gire muy rápido
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

            //  LÓGICA DE MOVIMIENTO 
            if (!onAttack && distance > 1.5f)
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
                // Frenamos al agente para que no siga empujando
                agent.isStopped = true;

                if (animator != null && hasMoveParam) animator.SetBool("Move", false);
            }

            // Si está cerca o atacando, lo forzamos a mirar al jugador suavemente
            if (distance <= 2f || onAttack)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0; // Ignoramos la altura para que no se incline hacia el piso

                if (directionToPlayer != Vector3.zero)
                {
                    // Rotación suave pero rápida hacia el jugador
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
                }
            }

            // LÓGICA DE ATAQUE 
            if (distance <= 1.5f || onAttack)
            {
                // Iniciar el ataque
                if (Time.time >= lastAttackTime + attackCooldown && !onAttack)
                {
                    if (animator != null) animator.SetTrigger("Attack");

                    lastAttackTime = Time.time;
                    lastFrameTime = Time.time;

                    onAttack = true;
                }

                // Aplicar el daño con un retraso (para que coincida con la animación del golpe)
                if (Time.time >= lastFrameTime + 0.3f && onAttack)
                {
                    // Volvemos a comprobar si el jugador no se escapó (dash) en este medio segundo
                    // Damos un pequeño margen extra (2f) para que el golpe sea un poco más generoso
                    if (distance <= 2f)
                    {
                        PlayerHealth health = player.GetComponent<PlayerHealth>();

                        if (health != null)
                        {
                            health.TakeDamage(attackDamage);
                        }
                    }

                    lastFrameTime = Time.time;
                    onAttack = false;
                }
            }
        }
    }
}