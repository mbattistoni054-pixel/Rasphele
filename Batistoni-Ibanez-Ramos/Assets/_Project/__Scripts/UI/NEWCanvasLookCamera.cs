using UnityEngine;
using TMPro;
using PatronesAplicados;

public class NEWCanvasLookCamera : MonoBehaviour
{
    public const string ShowInteractTextEvent = "ShowInteractText";

    public TextMeshProUGUI useText;

    private Transform _cachedCameraTransform;
    private bool _isSubscribed;

    private void Awake()
    {
        CacheMainCamera();
    }

    private void OnEnable()
    {
        CacheMainCamera();
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();

        if (useText != null)
        {
            useText.text = string.Empty;
        }
    }

    private void OnDisable()
    {
        if (_isSubscribed && EventManager.Instance != null)
        {
            EventManager.Instance.StopListening<string>(ShowInteractTextEvent, ShowText);
        }
        _isSubscribed = false;
    }

    private void LateUpdate()
    {
        if (_cachedCameraTransform == null)
        {
            CacheMainCamera();
            if (_cachedCameraTransform == null) return;
        }

        transform.rotation = _cachedCameraTransform.rotation;
    }

    private void CacheMainCamera()
    {
        if (_cachedCameraTransform == null && Camera.main != null)
        {
            _cachedCameraTransform = Camera.main.transform;
        }
    }

    private void TrySubscribe()
    {
        if (_isSubscribed || EventManager.Instance == null) return;

        EventManager.Instance.StartListening<string>(ShowInteractTextEvent, ShowText);
        _isSubscribed = true;
    }

    private void ShowText(string text)
    {
        if (useText != null)
        {
            useText.text = text;
        }
    }
}