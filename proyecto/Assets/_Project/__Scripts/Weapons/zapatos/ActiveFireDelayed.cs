using UnityEngine;
using PatronesAplicados;

public class ActiveFireDelayed : MonoBehaviour, IPausable
{
    public float timeEnd = 0.4f;

    private float _actualTime;
    private bool _hasActivated;
    private bool _isPaused;
    private SphereCollider _cachedCollider;

    private void Awake()
    {
        _cachedCollider = GetComponent<SphereCollider>();
    }

    private void OnEnable()
    {
        _actualTime = 0f;
        _hasActivated = false;
        _isPaused = PauseManager.IsPaused;

        if (_cachedCollider == null) _cachedCollider = GetComponent<SphereCollider>();
        if (_cachedCollider != null) _cachedCollider.enabled = false;

        PauseManager.Register(this);
    }

    private void OnDisable()
    {
        PauseManager.Unregister(this);
    }

    private void Update()
    {
        if (_hasActivated || _isPaused) return;

        _actualTime += Time.deltaTime;
        if (_actualTime >= timeEnd)
        {
            _hasActivated = true;
            if (_cachedCollider != null)
            {
                _cachedCollider.enabled = true;
            }
        }
    }

    public void OnPause() => _isPaused = true;
    public void OnResume() => _isPaused = false;
}