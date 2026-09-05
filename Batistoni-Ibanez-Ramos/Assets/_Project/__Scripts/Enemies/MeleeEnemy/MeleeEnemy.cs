using UnityEngine;


public class MeleeEnemy : EnemyBase
{
    [Header("Estadísticas de Ataque")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f; 

    private float lastAttackTime;

    private void FixedUpdate()
    {

        // Si por herencia o errores pierde su Rigidbody o velocidad, lo reparamos.
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (currentSpeed <= 0) currentSpeed = baseSpeed;

        // Si definitivamente no tiene un componente físico en Unity, cancelamos para no romper el juego
        if (rb == null) return;


        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return; // Si aún no existe, no hacemos nada en este frame
        }

        // Si hay jugador y no estamos aturdidos por un efecto del arma
        if (player != null && !isStunned)
        {
            // 1. Calculamos la dirección hacia el jugador
            Vector3 direction = (player.position - transform.position).normalized;

            // 2. Mantenemos la velocidad Y intacta para que la gravedad actúe
            Vector3 targetVelocity = new Vector3(direction.x * currentSpeed, rb.linearVelocity.y, direction.z * currentSpeed);

            // 3. Aplicamos la velocidad al Rigidbody
            rb.linearVelocity = targetVelocity;
        }
    }

    // Detecta colisiones físicas con otros objetos
    private void OnCollisionStay(Collision collision)
    {
        // Si el enemigo está aturdido (Stun), no puede atacar
        if (isStunned) return;

        // Comprobamos si con lo que estamos chocando es el Jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Comprobamos si ya pasó suficiente tiempo desde el último golpe
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                // Buscamos el script de vida del jugador
                PlayerHealth pHealth = collision.gameObject.GetComponent<PlayerHealth>();

                if (pHealth != null)
                {
                    pHealth.TakeDamage(attackDamage);
                    lastAttackTime = Time.time; // Reiniciamos el reloj del ataque
                }
            }
        }
    }
}