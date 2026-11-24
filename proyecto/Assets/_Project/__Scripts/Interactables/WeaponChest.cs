using UnityEngine;
using UnityEngine.InputSystem; 

public class WeaponChest : MonoBehaviour
{
    [Header("Recompensa del Cofre")]
    [Tooltip("Arrastra aquí el PREFAB del arma que dará este cofre (ej. Zapatos de Fuego)")]
    public GameObject weaponPrefabToGive;


    private bool isPlayerNear = false;
    private Transform playerTransform;

    void Update()
    {
        if (isPlayerNear && Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                OpenChest();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerTransform = other.transform; 

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerTransform = null;

        }
    }

    private void OpenChest()
    {
        if (weaponPrefabToGive != null && playerTransform != null)
        {
            Instantiate(weaponPrefabToGive, playerTransform.position, Quaternion.identity, playerTransform);

            Debug.Log($"¡Cofre abierto! Obtuviste: {weaponPrefabToGive.name}");
        }
        else
        {
            Debug.LogWarning("El cofre no tiene un arma asignada en el Inspector.");
        }

        gameObject.SetActive(false);
    }
}