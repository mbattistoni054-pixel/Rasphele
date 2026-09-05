using UnityEngine;

public class ActiveFireDelayed : MonoBehaviour
{

    float timeEnd = 0.4f;
    float actualTime;

    private void Update()
    {

        actualTime += Time.deltaTime;

        if (actualTime >= timeEnd)
        {
            gameObject.GetComponent<SphereCollider>().enabled = true;
            Destroy(this);
        }

    }



}
