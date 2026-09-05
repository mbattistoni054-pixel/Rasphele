using UnityEngine;
using TMPro;

public class CanvasLookCamera : MonoBehaviour
{

    public TextMeshProUGUI useText; 

    private void Start()
    {
        HUDManager.Instance.interactText = useText;
        useText.text = null;
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
