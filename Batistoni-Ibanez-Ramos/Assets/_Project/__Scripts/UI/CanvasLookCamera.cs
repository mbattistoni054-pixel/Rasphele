using PatronesAplicados;
using TMPro;
using UnityEngine;

public class CanvasLookCamera : MonoBehaviour  //revisar este script
{

    public TextMeshProUGUI useText; 

    private void Start()
    {
       
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

        }

    }
}
