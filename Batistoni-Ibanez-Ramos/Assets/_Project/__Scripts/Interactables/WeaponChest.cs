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
        // Si el jugador está cerca y presiona la tecla E
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
            playerTransform = other.transform; // Guardamos quién es el jugador

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
            // Creamos el arma exactamente en la posición del jugador, y le decimos
            // que su nuevo "padre" (parent) es el jugador. Así lo seguirá a donde vaya.
            Instantiate(weaponPrefabToGive, playerTransform.position, Quaternion.identity, playerTransform);

            Debug.Log($"¡Cofre abierto! Obtuviste: {weaponPrefabToGive.name}");
        }
        else
        {
            Debug.LogWarning("El cofre no tiene un arma asignada en el Inspector.");
        }


        // Destruimos el cofre 
        Destroy(gameObject);
    }
}