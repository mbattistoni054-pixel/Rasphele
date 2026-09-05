using UnityEngine;
using UnityEngine.EventSystems;

// Este script requiere que el objeto tenga un componente de UI que bloquee el raycast (como Image)
public class ButtonAudioTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    // Se ejecuta automáticamente cuando el mouse entra al área del botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MenuAudioManager.Instance != null)
        {
            MenuAudioManager.Instance.PlayHoverSound();
        }
    }

    // Se ejecuta automáticamente cuando se hace clic en el botón
    public void OnPointerClick(PointerEventData eventData)
    {
        if (MenuAudioManager.Instance != null)
        {
            MenuAudioManager.Instance.PlayClickSound();
        }
    }
}
