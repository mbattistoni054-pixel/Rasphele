using System.Collections;
using PatronesAplicados;
using UnityEngine;

public class EnemyVisual : MonoBehaviour, IObserver
{
    [SerializeField] EnemyBaseRefactored enemyBase;
    [SerializeField] Collider enemyCollider;
    [SerializeField] Animator enemyAnimator;

    [SerializeField] float spawnSpeed = 2;
    [SerializeField] float hitSpeed = 4;

    [SerializeField] private Renderer[] renderers;

    private void OnEnable()
    {
        enemyBase.Subscribe(this);

        foreach (var renderer in renderers)
        {
            renderer.material.SetFloat("_DeathDissolve", 0);
            renderer.material.SetFloat("_GetHit", 0);
        }

        dieCoroutine = null;

        gameObject.layer = 0;

        if (enemyBase == null) enemyBase = GetComponent<EnemyBaseRefactored>();
        enemyBase.enabled = false;

        if (enemyCollider == null) enemyCollider = GetComponent<Collider>();
        enemyCollider.enabled = false;

        if (enemyAnimator == null) enemyAnimator = GetComponent<Animator>();
        enemyAnimator.enabled = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnCoroutine());
    }


    private Coroutine spawnCoroutine;
    IEnumerator SpawnCoroutine()
    {
        float time = 0f;
        while (time <= 3)
        {
            time += Time.deltaTime * spawnSpeed;
            foreach (var renderer in renderers)
            {
                renderer.material.SetFloat("_Appareance", time);
            }
            yield return null;
        }
        enemyBase.enabled = true;
        enemyCollider.enabled = true;
        enemyAnimator.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("EnemyLayer");
    }


    private Coroutine hitCoroutine;
    IEnumerator HitCoroutine()
    {
        float time = 0f;
        while (time <= 2)
        {
            time += Time.deltaTime * hitSpeed;
            foreach (var renderer in renderers)
            {
                if (time <= 1)
                {
                    renderer.material.SetFloat("_GetHit", time);
                }
                else
                {
                    renderer.material.SetFloat("_GetHit", 2 - time);
                }
            }
            yield return null;
        }
    }


    private Coroutine dieCoroutine;
    IEnumerator DieCoroutine()
    {
        gameObject.layer = 0;
        enemyBase.enabled = false;
        enemyCollider.enabled = false;
        enemyAnimator.enabled = false;
        float time = 0f;
        while (time <= 1)
        {
            time += Time.deltaTime * hitSpeed;
            foreach (var renderer in renderers)
            {
                renderer.material.SetFloat("_GetHit", time * 2);
                renderer.material.SetFloat("_DeathDissolve", time);
            }
            yield return null;
        }
        enemyBase.ReturnEnemy();
    }


    public void OnNotify(string action)
    {
        switch (action)
        {
            case "TakeDamage":
                if (hitCoroutine != null)
                {
                    StopCoroutine(hitCoroutine);
                }
                if (dieCoroutine == null) hitCoroutine = StartCoroutine(HitCoroutine());
                break;

            case "Die":
                if (dieCoroutine != null)
                {
                    StopCoroutine(dieCoroutine);
                }
                dieCoroutine = StartCoroutine(DieCoroutine());
                break;
        }
    }
}
