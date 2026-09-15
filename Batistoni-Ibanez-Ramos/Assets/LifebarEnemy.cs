using PatronesAplicados;
using UnityEngine;
using UnityEngine.UI;

public class LifebarEnemy : MonoBehaviour, IObserver
{

    [SerializeField] private EnemyBaseRefactored enemy;
    [SerializeField] private Image image;


    private void OnEnable()
    {
        if (enemy != null)
        {
            enemy.Subscribe(this);
        }
        image.fillAmount = 1;
    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.Unsubscribe(this);
        }
    }

    public void OnNotify(string action)
    {
        if (action == "Damage")
        {
            image.fillAmount = enemy.CurrentHealth / enemy.maxHealth;
        }
    }

    void Update()
    {
        // la barra gire mirando a la camara siempre
    }
}
