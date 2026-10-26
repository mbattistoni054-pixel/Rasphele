using UnityEngine;
using UnityEngine.InputSystem;

namespace PatronesAplicados
{
    public class PuertaRefactored : MonoBehaviour
    {
        private bool _playerClose;
        private bool _bossSpawned;
        private bool _doorOpened;

        public GameObject boss;
        public GameObject bossCheck;
        public GameObject barrier;
        public GameObject winZone;
        public Transform spawnPoint;
        public GameObject Spawner;

        private void Update()
        {
            if (_playerClose && !_bossSpawned && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                SpawnBoss();
            }

            if (_bossSpawned && !_doorOpened && bossCheck == null)
            {
                _doorOpened = true;
                if (barrier != null) barrier.SetActive(false);
                if (winZone != null) winZone.SetActive(true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !_bossSpawned)
            {
                _playerClose = true;
                ShowInteractMessage("¿Desafiar a la montaña?\nPress E");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerClose = false;
                ShowInteractMessage(string.Empty);
            }
        }

        private void SpawnBoss()
        {
            _bossSpawned = true;
            ShowInteractMessage("Jefe Invocado");

            if (Spawner != null) Spawner.SetActive(false);
            if (boss != null && spawnPoint != null)
            {
                bossCheck = Instantiate(boss, spawnPoint.position, Quaternion.identity);
            }
        }

        private static void ShowInteractMessage(string message)
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.ShowInteractText, message);
            }
        }
    }
}