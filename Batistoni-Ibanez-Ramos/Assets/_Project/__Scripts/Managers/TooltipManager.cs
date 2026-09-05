    using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI; 

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("Referencias")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    private RectTransform panelRect;
    private RectTransform parentRect;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (tooltipPanel != null)
        {
            panelRect = tooltipPanel.GetComponent<RectTransform>();

            Canvas rootCanvas = tooltipPanel.GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                parentRect = rootCanvas.GetComponent<RectTransform>();
            }
            else
            {
                parentRect = tooltipPanel.transform.parent.GetComponent<RectTransform>();
            }

            tooltipPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf && Mouse.current != null && parentRect != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                mousePos,
                null,
                out Vector2 localPoint);

            // Sumamos 15 píxeles a la derecha y 15 hacia abajo. 
            // Así la flecha del ratón nunca choca físicamente con el panel negro.
            localPoint += new Vector2(15f, -15f);

            panelRect.localPosition = localPoint;

            float pivotX = mousePos.x / Screen.width > 0.6f ? 1f : 0f;
            float pivotY = mousePos.y / Screen.height > 0.5f ? 1f : 0f;

            panelRect.pivot = new Vector2(pivotX, pivotY);
        }
    }

    public void ShowTooltip(string title, string description)
    {
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

        // Forzamos a los Layout Groups a recalcular su tamaño AL INSTANTE en este mismo milisegundo
        if (panelRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        }

        if (tooltipPanel != null) tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        if (titleText != null) titleText.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }
}