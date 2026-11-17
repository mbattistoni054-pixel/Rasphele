using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PatronesAplicados;

public class NEWItemRewardUI : MonoBehaviour
{
    public static NEWItemRewardUI Instance { get; private set; }

    private static readonly Color ExtraordinaryColor = new Color(1f, 0.5f, 0f);

    [Header("Elementos Visuales")]
    public GameObject panel;
    public Image itemIcon;
    public Image border;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescText;
    public TextMeshProUGUI tierText;

    private ItemData _currentItem;
    private PlayerInventory _playerInventory;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);
    }

    private void Start()
    {
        CachePlayerInventory();
    }

    private void CachePlayerInventory()
    {
        if (_playerInventory != null) return;

        Transform playerTransform = GameManagerRefactored.Instance != null
            ? GameManagerRefactored.Instance.Player
            : null;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        if (playerTransform != null)
        {
            playerTransform.TryGetComponent(out _playerInventory);
        }
    }

    public void ShowReward(ItemData item)
    {
        if (item == null) return;

        _currentItem = item;
        if (itemIcon != null) itemIcon.sprite = item.icon;
        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemDescText != null) itemDescText.text = item.description;

        switch (item.tier)
        {
            case ItemTier.Comun:
                SetTierVisuals("COMÚN", Color.cyan);
                break;
            case ItemTier.Raro:
                SetTierVisuals("RARO", Color.magenta);
                break;
            case ItemTier.Extraordinario:
                SetTierVisuals("EXTRAORDINARIO", ExtraordinaryColor);
                break;
        }

        if (panel != null) panel.SetActive(true);

        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerEvent(GameEventKeys.PauseRequested);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetTierVisuals(string label, Color color)
    {
        if (tierText != null)
        {
            tierText.text = label;
            tierText.color = color;
        }
        if (border != null)
        {
            border.color = color;
        }
    }

    public void KeepItem()
    {
        if (_currentItem == null) return;

        CachePlayerInventory();
        if (_playerInventory != null)
        {
            _playerInventory.AddItem(_currentItem);
        }
        ClosePanel();
    }

    public void SellItem()
    {
        if (_currentItem == null) return;

        if (PlayerStatsRefactored.Instance != null)
        {
            PlayerStatsRefactored.Instance.AddMoney(25);
        }

        ClosePanel();
    }

    private void ClosePanel()
    {
        _currentItem = null;
        if (panel != null) panel.SetActive(false);

        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerEvent(GameEventKeys.ResumeRequested);
        }
    }
}