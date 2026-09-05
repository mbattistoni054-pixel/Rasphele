using UnityEngine;
using UnityEngine.InputSystem;

public class Puerta : MonoBehaviour
{
 
    bool playerClose = false;
    bool bossSpawned = false;
    public GameObject boss;
    public GameObject bossCheck;
    public GameObject barrier;
    public GameObject winZone;
    public Transform spawnPoint;

    public GameObject Spawner;

    // Update is called once per frame
    void Update()
    {
        
        if (playerClose && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && bossSpawned == false)
        {
            bossSpawned = true;
            SpawnBoss();
        }

        if (bossSpawned)
        {
            if (bossCheck == null)
            {
                barrier.SetActive(false);
                winZone.SetActive(true);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = true;

            string txt = "¿Desafiar a la montaña?\nPress E";
            HUDManager.Instance.InteractText(txt);
        }


    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = false;

            HUDManager.Instance.InteractText(null);
        }
    }

    private void SpawnBoss()
    {
        string txt = "Jefe Invocado";
        HUDManager.Instance.InteractText(txt);

        Spawner.SetActive(false);

        bossSpawned = true;

        bossCheck = Instantiate(boss, spawnPoint.position, Quaternion.identity);
    }

}
