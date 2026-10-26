using UnityEngine;
using TMPro;

namespace PatronesAplicados
{
    public class MagnetStructure : MonoBehaviour
    {
        public GameObject pressE_Text;
        public TextMeshPro floatingCostText;

        private Transform _playerTransform;
        private bool _isActivated;

        private void OnEnable()
        {
            _isActivated = false;
            _playerTransform = null;

            if (pressE_Text != null) pressE_Text.SetActive(false);
            if (floatingCostText != null) floatingCostText.text = "Gratis";
        }

        private void Update()
        {
            transform.Rotate(0f, 90f * Time.deltaTime, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isActivated) return;

            if (other.CompareTag("Player"))
            {
                _isActivated = true;
                _playerTransform = other.transform;

                ActivateMagnet();
                gameObject.SetActive(false);
            }
        }

        private void ActivateMagnet()
        {
            if (_playerTransform == null) return;

            NEWExperienceOrb[] allOrbs = Object.FindObjectsByType<NEWExperienceOrb>(FindObjectsSortMode.None);

            for (int i = 0; i < allOrbs.Length; i++)
            {
                if (allOrbs[i] != null && allOrbs[i].gameObject.activeInHierarchy)
                {
                    allOrbs[i].ForceAttract(_playerTransform);
                }
            }
        }
    }
}