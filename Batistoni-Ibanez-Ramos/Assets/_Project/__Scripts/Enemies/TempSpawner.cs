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
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f; 
        }
    }

    private void SpawnEnemy()
    {
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject selectedEnemy = enemyPrefabs[randomIndex];

        Instantiate(selectedEnemy, transform.position, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}