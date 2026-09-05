using UnityEngine;

public class TempSpawner : MonoBehaviour
{
    [Header("Configuración de Spawns")]
    [Tooltip("Arrastra aquí los diferentes prefabs de enemigos que quieres que aparezcan.")]
    public GameObject[] enemyPrefabs;

    [Tooltip("Cada cuántos segundos aparecerá un nuevo enemigo.")]
    public float spawnInterval = 2f;

    private float timer = 16f;

    void Update()
    {
        // Si no hay enemigos asignados, no hacemos nada para evitar errores
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f; // Reiniciamos el reloj
        }
    }

    private void SpawnEnemy()
    {
        // Elegimos un enemigo al azar de la lista 
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject selectedEnemy = enemyPrefabs[randomIndex];

        // Instanciamos el enemigo en la posición exacta de este Spawner
        Instantiate(selectedEnemy, transform.position, Quaternion.identity);
    }

    // DIBUJADO DEL GIZMO EN LA ESCENA 
    private void OnDrawGizmos()
    {

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}