using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
  


   public void ChangeScene(string sceneName)
    {

        SceneManager.LoadScene(sceneName);

    }


    public void Quit()
    {

        Debug.Log("Quit Game"); 
        Application.Quit();

    }

}
