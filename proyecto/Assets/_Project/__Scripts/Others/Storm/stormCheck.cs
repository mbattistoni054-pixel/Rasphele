using UnityEngine;

public class stormCheck : MonoBehaviour
{

    public RingStormManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            manager.isInStorm = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            manager.isInStorm = false;
        }
    }

}
