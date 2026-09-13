using UnityEngine;
using UnityEngine.InputSystem;

namespace PatronesAplicados
{
    public class PuertaRefactored : MonoBehaviour
    {
        bool playerClose = false;
        bool bossSpawned = false;
        public GameObject boss;
        public GameObject bossCheck;
        public GameObject barrier;
        public GameObject winZone;
        public Transform spawnPoint;

        public GameObject Spawner;

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
                string txt = "Desafiar a la montaa?\nPress E";
                if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", txt);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerClose = false;
                if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", "");
            }
        }

        private void SpawnBoss()
        {
            string txt = "Jefe Invocado";
            if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("ShowInteractText", txt);

            Spawner.SetActive(false);
            bossSpawned = true;
            bossCheck = Instantiate(boss, spawnPoint.position, Quaternion.identity);
        }
    }
}


