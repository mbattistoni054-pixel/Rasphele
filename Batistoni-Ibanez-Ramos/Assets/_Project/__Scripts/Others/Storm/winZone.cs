using PatronesAplicados;
using UnityEngine;

public class winZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //GameManagerRefactored.Instance.EnablePause();
          //  EventManager.Instance.TriggerEvent("PauseRequested");
            EventManager.Instance.TriggerEvent("LevelComplete");
           // GameManagerRefactored.Instance.levelCompleteCanvas.SetActive(true);
        }
    }

}
