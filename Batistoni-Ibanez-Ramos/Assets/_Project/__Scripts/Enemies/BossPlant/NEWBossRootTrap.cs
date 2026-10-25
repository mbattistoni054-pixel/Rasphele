using System.Collections;
using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWBossRootTrap : MonoBehaviour, IPausable
{
    [Header("Tiempos del Ataque")]
    public float followTime = 2f;
    public float lockWarningTime = 0.5f;
    public float rootDuration = 2f;

    [Header("Área y Efectos")]
    public float trapRadius = 4f;
    public Transform warningVisual;
    public Transform rootVisual;

    private Transform _playerTarget;
    private float _damage;
    private bool _isPaused;

    private Renderer _warningRenderer;
    private Color _originalWarningColor = new Color(1f, 0.5f, 0f, 0.5f);
    private MaterialPropertyBlock _propBlock;
    private bool _isVisualInitialized;

    private readonly Collider[] _hitBuffer = new Collider[16];
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void OnEnable()
    {
        _isPaused = PauseManager.IsPaused;
        PauseManager.Register(this);
    }

    private void InitializeVisualCache()
    {
        if (_isVisualInitialized) return;
        _isVisualInitialized = true;

        _propBlock = new MaterialPropertyBlock();

        if (warningVisual != null)
        {
            _warningRenderer = warningVisual.GetComponent<Renderer>();
            if (_warningRenderer != null && _warningRenderer.sharedMaterial != null)
            {
                if (_warningRenderer.sharedMaterial.HasProperty(BaseColorId))
                    _originalWarningColor = _warningRenderer.sharedMaterial.GetColor(BaseColorId);
                else if (_warningRenderer.sharedMaterial.HasProperty(ColorId))
                    _originalWarningColor = _warningRenderer.sharedMaterial.GetColor(ColorId);
            }
        }
    }

    public void Setup(Transform target, float dmg)
    {
        InitializeVisualCache();

        _playerTarget = target;
        _damage = dmg;

        if (warningVisual != null)
        {
            warningVisual.localScale = new Vector3(trapRadius * 2f, 0.1f, trapRadius * 2f);
            warningVisual.gameObject.SetActive(true);
            SetWarningColor(_originalWarningColor);
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
        if (_warningRenderer == null || _propBlock == null) return;

        _warningRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(BaseColorId, c);
        _propBlock.SetColor(ColorId, c);
        _warningRenderer.SetPropertyBlock(_propBlock);
    }

    private void OnDisable()
    {
        PauseManager.Unregister(this);
        StopAllCoroutines();
    }

    private IEnumerator WaitPausableSeconds(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (!_isPaused)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
    }

    private IEnumerator TrapRoutine()
    {
        float timer = 0f;
        while (timer < followTime)
        {
            if (!_isPaused)
            {
                if (_playerTarget != null)
                {
                    Vector3 targetPos = new Vector3(_playerTarget.position.x, _playerTarget.position.y + 0.1f, _playerTarget.position.z);
                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
                }
                timer += Time.deltaTime;
            }
            yield return null;
        }

        SetWarningColor(Color.red);

        yield return StartCoroutine(WaitPausableSeconds(lockWarningTime));

        if (warningVisual != null) warningVisual.gameObject.SetActive(false);
        if (rootVisual != null) rootVisual.gameObject.SetActive(true);

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, trapRadius, _hitBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _hitBuffer[i];
            if (hit != null && hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(_damage);
                }

                if (hit.TryGetComponent(out IRootable rootable))
                {
                    rootable.ApplyRoot(rootDuration);
                }
                else if (hit.TryGetComponent(out IKnockbackable knockbackable))
                {
                    knockbackable.ApplyKnockback(Vector3.zero, rootDuration);
                }
            }
        }

        yield return StartCoroutine(WaitPausableSeconds(rootDuration));

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

    public void OnPause() => _isPaused = true;
    public void OnResume() => _isPaused = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, trapRadius);
    }
}