using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MagnetStructure : MonoBehaviour
{
    public GameObject pressE_Text;
    public TextMeshPro floatingCostText;

    private Transform _playerTransform;

    void Start()
    {
        if (pressE_Text != null) pressE_Text.SetActive(false);
        if (floatingCostText != null) floatingCostText.text = "Gratis";
    }

    private void Update()
    {
        gameObject.transform.Rotate(0, 90 * Time.deltaTime, 0);   
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerTransform = other.transform;

            ActivateMagnet();

            Destroy(gameObject);
        }
    }


    private void ActivateMagnet()
    {
        if (_playerTransform == null) return;

        // Busca todas las gemas de experiencia en la escena entera
        NEWExperienceOrb[] allOrbs = Object.FindObjectsByType<NEWExperienceOrb>(FindObjectsSortMode.None);

        int count = 0;
        foreach (NEWExperienceOrb orb in allOrbs)
        {
            orb.ForceAttract(_playerTransform);
            count++;
        }

        Debug.Log($"�Im�n activado! Atrayendo {count} gemas.");

        // Destruimos el im�n tras usarlo
        Destroy(gameObject);
    }
}