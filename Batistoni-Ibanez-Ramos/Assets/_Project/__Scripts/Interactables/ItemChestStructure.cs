using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ItemChestStructure : MonoBehaviour
{
    [Header("Configuración del Cofre")]
    public bool isSpecialChest = false;
    public int cost = 50;

    [Header("UI del Cofre")]
    
    private bool isPlayerNear = false;


    void Update()
    {
        if (isPlayerNear && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryOpenChest();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;

            if (isSpecialChest)
            {
                string txt = "Press E\n$50\n(25%)";
                HUDManager.Instance.InteractText(txt);
            }
            else
            {
                string txt = "Press E\n$50";
                HUDManager.Instance.InteractText(txt);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;

            if (HUDManager.Instance != null && HUDManager.Instance.interactText != null)
            {
                HUDManager.Instance.interactText.text = "";
            }
        }
    }

    private void TryOpenChest()
    {
        if (PlayerStats.Instance == null || ItemManager.Instance == null || ItemRewardUI.Instance == null) return;

        if (PlayerStats.Instance.SpendMoney(cost))
        {
            //Tirada de Suerte para el Cofre Especial
            if (isSpecialChest)
            {
                if (Random.Range(0f, 100f) > 25f)
                {
                    Debug.Log("El cofre especial falló al abrirse. Dinero perdido.");

                    string txt = "El cofre fallo al abrirse.\nPress E\n$50\n(25%)";
                    HUDManager.Instance.InteractText(txt);

                    // Efecto visual de fallo aquí (ruido de error)
                    return; // Terminamos aquí, el jugador perdió sus 50.
                }
            }

            // Si llegamos aquí (o es normal, o es especial y tuvo éxito), generamos el objeto
            ItemTier rolledTier = RollTier();
            ItemData rewardedItem = ItemManager.Instance.GetRandomItem(rolledTier);

            if (rewardedItem != null)
            {
                ItemRewardUI.Instance.ShowReward(rewardedItem);
            }
            else
            {
                Debug.LogWarning($"No hay objetos en la lista de {rolledTier} en el ItemManager.");
            }

            HUDManager.Instance.InteractText(null);

            // El cofre se destruye (o se desactiva) tras abrirse con éxito
            Destroy(gameObject);
        }
        else
        {
            string txt = "No tienes suficiente dinero.";
            HUDManager.Instance.InteractText(txt);
        }
    }

    private ItemTier RollTier()
    {
        float roll = Random.Range(0f, 100f);

        if (!isSpecialChest)
        {
            // Probabilidades Normales: 75% Común, 20% Raro, 5% Extraordinario
            if (roll <= 75f) return ItemTier.Comun;
            if (roll <= 95f) return ItemTier.Raro;
            return ItemTier.Extraordinario;
        }
        else
        {
            // Probabilidades Especiales: 80% Raro, 20% Extraordinario
            if (roll <= 80f) return ItemTier.Raro;
            return ItemTier.Extraordinario;
        }
    }
}