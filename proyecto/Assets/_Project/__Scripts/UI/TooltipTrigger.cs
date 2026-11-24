using UnityEngine;
using UnityEngine.EventSystems;

// IPointerEnter y Exit detectan cuando el ratón entra y sale del icono
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Información a Mostrar")]
    public string headerTitle;
    [TextArea]
    public string contentDescription;

    // Puedes llamar a esta función desde tu script de Inventario para actualizar 
    // la info si el objeto cambia dinámicamente.
    public void SetupTooltip(string title, string desc)
    {
        headerTitle = title;
        contentDescription = desc;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(headerTitle, contentDescription);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}