using System.Collections;
using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWBossRootTrap : MonoBehaviour
{
    [Header("Tiempos del Ataque")]
    public float followTime = 2f;
    public float lockWarningTime = 0.5f;
    public float rootDuration = 2f;

    [Header("Área y Efectos")]
    public float trapRadius = 4f;
    public Transform warningVisual;
    public Transform rootVisual;

    private Transform playerTarget;
    private float damage;
    private PlayerControllerRefactored trappedPlayerCtrl;

    private Renderer warningRenderer;
    private Color originalWarningColor = new Color(1f, 0.5f, 0f, 0.5f);
    private MaterialPropertyBlock propBlock;
    private bool isVisualInitialized = false;

    private readonly Collider[] _hitBuffer = new Collider[16];
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void InitializeVisualCache()
    {
        if (isVisualInitialized) return;
        isVisualInitialized = true;

        propBlock = new MaterialPropertyBlock();

        if (warningVisual != null)
        {
            warningRenderer = warningVisual.GetComponent<Renderer>();
            if (warningRenderer != null && warningRenderer.sharedMaterial != null)
            {
                if (warningRenderer.sharedMaterial.HasProperty(BaseColorId))
                    originalWarningColor = warningRenderer.sharedMaterial.GetColor(BaseColorId);
                else if (warningRenderer.sharedMaterial.HasProperty(ColorId))
                    originalWarningColor = warningRenderer.sharedMaterial.GetColor(ColorId);
            }
        }
    }

    public void Setup(Transform target, float dmg)
    {
        InitializeVisualCache();

        playerTarget = target;
        damage = dmg;

        if (warningVisual != null)
        {
            warningVisual.localScale = new Vector3(trapRadius * 2f, 0.1f, trapRadius * 2f);
            warningVisual.gameObject.SetActive(true);
            SetWarningColor(originalWarningColor);
        }

        if (rootVisual != null)
        {
            rootVisual.gameObject.SetActive(false);
        }

        StopAllCoroutines();
        StartCoroutine(TrapRoutine());
    }

    private void SetWarningColor(Color c)
    {
        if (warningRenderer == null || propBlock == null) return;

        warningRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(BaseColorId, c);
        propBlock.SetColor(ColorId, c);
        warningRenderer.SetPropertyBlock(propBlock);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        ReleasePlayer();
    }

    private IEnumerator TrapRoutine()
    {
        float timer = 0f;
        while (timer < followTime)
        {
            if (playerTarget != null)
            {
                Vector3 targetPos = new Vector3(playerTarget.position.x, playerTarget.position.y + 0.1f, playerTarget.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        SetWarningColor(Color.red);

        yield return new WaitForSeconds(lockWarningTime);

        if (warningVisual != null) warningVisual.gameObject.SetActive(false);
        if (rootVisual != null) rootVisual.gameObject.SetActive(true);

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, trapRadius, _hitBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _hitBuffer[i];
            if (hit != null && hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent(out PlayerHealthRefactored refHealth))
                    refHealth.TakeDamage(damage);

                if (hit.TryGetComponent(out PlayerControllerRefactored playerV2))
                {
                    playerV2.ApplyKnockback(Vector3.zero, rootDuration);
                }
            }
        }

        yield return new WaitForSeconds(rootDuration);

        if (rootVisual != null) rootVisual.gameObject.SetActive(false);

        if (ProjectilePoolManager.Instance != null)
        {
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        ReleasePlayer();
    }

    private void ReleasePlayer()
    {
        if (trappedPlayerCtrl != null)
        {
            trappedPlayerCtrl.enabled = true;
            trappedPlayerCtrl = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, trapRadius);
    }
}