using UnityEngine;
using TMPro;

public class CanvasLookCamera : MonoBehaviour
{

    public TextMeshProUGUI useText; 

    private void Start()
    {
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.interactText = useText;
        }
        if (useText != null)
        {
            useText.text = null;
        }
    }

    void Update()
    {
        
        if (Camera.main != null )
        {

            transform.rotation = Camera.main.transform.rotation;

            //Debug.Log("CAMARA MAIN");
        }

    }
}
