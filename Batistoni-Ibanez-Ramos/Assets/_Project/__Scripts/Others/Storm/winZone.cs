using UnityEngine;

public class winZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.EnablePause();
            GameManager.Instance.levelCompleteCanvas.SetActive(true);
        }
    }

}
