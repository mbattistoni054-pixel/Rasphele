using UnityEngine;
using UnityEngine.AI;

public class EnemySquadSpawner : MonoBehaviour
{
    [Header("Configuración del Escuadrón")]
    [Tooltip("El prefab del enemigo que quieres que aparezca en grupo.")]
    public GameObject enemyPrefab;

    [Tooltip("Cuántos van a aparecer de golpe.")]
    public int amountToSpawn = 3;

    [Tooltip("Qué tan separados aparecerán entre sí.")]
    public float spawnRadius = 2f;

    [Header("Efectos Visuales")]
    public GameObject spawnSmokePrefab; 

    void Start()
    {
        // Buscamos al jugador para pasárselo a los enemigos
        Transform player = null;
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        for (int i = 0; i < amountToSpawn; i++)
        {
            // Calculamos una posición aleatoria alrededor de este punto
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
            {

                if (spawnSmokePrefab != null)
                {
                    Instantiate(spawnSmokePrefab, hit.position, Quaternion.identity);
                }

                // Creamos al mini-enemigo
                GameObject newEnemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);

                // Le damos la orden de seguir al jugador
                if (player != null)
                {
                    EnemyBase enemyScript = newEnemy.GetComponent<EnemyBase>();
                    if (enemyScript != null) enemyScript.SetTarget(player);
                }
            }
        }

        // El invocador ya hizo su trabajo, se destruye de inmediato sin dejar rastro
        Destroy(gameObject);
    }
}