using PatronesAplicados;
using UnityEngine;

public class winZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.Instance.TriggerEvent("LevelComplete");
        }
    }

}
