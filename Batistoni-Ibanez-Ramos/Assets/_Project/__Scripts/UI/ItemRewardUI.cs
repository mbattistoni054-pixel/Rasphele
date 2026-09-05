using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemRewardUI : MonoBehaviour
{
    public static ItemRewardUI Instance;

    [Header("Elementos Visuales")]
    public GameObject panel;
    public Image itemIcon;
    public Image border;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescText;
    public TextMeshProUGUI tierText;

    private ItemData currentItem;
    private PlayerInventory playerInventory;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerInventory = player.GetComponent<PlayerInventory>();
    }

    public void ShowReward(ItemData item)
    {
        currentItem = item;
        itemIcon.sprite = item.icon;
        itemNameText.text = item.itemName;
        itemDescText.text = item.description;

        switch (item.tier)
        {
            case ItemTier.Comun: tierText.text = "COMÚN"; tierText.color = Color.cyan; border.color = Color.cyan; break;
            case ItemTier.Raro: tierText.text = "RARO"; tierText.color = Color.magenta; border.color = Color.magenta; break;
            case ItemTier.Extraordinario: tierText.text = "EXTRAORDINARIO"; tierText.color = new Color(1f, 0.5f, 0f); border.color = new Color(1f, 0.5f, 0f); break; // Naranja
        }

        panel.SetActive(true);
        GameManager.Instance.EnablePause(); // Pausamos el juego mientras decides
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Botón: Equipar
    public void KeepItem()
    {
        if (currentItem == null) return; 

        if (playerInventory != null)
        {
            playerInventory.AddItem(currentItem);
        }
        ClosePanel();
    }

    // Botón: Vender
    public void SellItem()
    {
        if (currentItem == null) return; 

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddMoney(25);
            Debug.Log("Objeto vendido por $25.");
        }
        ClosePanel();
    }

    private void ClosePanel()
    {
        currentItem = null; // Vaciamos el objeto para evitar trampas
        panel.SetActive(false);
        GameManager.Instance.DisablePause();
    }
}