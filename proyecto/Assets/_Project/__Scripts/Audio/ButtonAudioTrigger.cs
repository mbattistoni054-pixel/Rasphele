using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAudioTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MenuAudioManager.Instance != null)
        {
            MenuAudioManager.Instance.PlayHoverSound();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (MenuAudioManager.Instance != null)
        {
            MenuAudioManager.Instance.PlayClickSound();
        }
    }
}
