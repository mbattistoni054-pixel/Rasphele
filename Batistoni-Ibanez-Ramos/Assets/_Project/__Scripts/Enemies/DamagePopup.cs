using UnityEngine;
using TMPro;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class DamagePopup : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private DamageVisualConfigSO visualConfig;

    private TMP_Text _textMesh;
    private float _disappearTimer;
    private Color _textColor;
    private Vector3 _moveVector;
    private Transform _mainCamTransform;

    private void Awake()
    {
        _textMesh = GetComponentInChildren<TMP_Text>();

        if (_textMesh == null)
        {
            Debug.LogError("¡ERROR! El Prefab del DamagePopup no tiene ningún componente TextMeshPro adjunto.");
        }
    }

    public void Setup(float damageAmount, DamageType type, bool isCrit = false, bool isPlayer = false, bool isBleed = false)
    {
        if (_textMesh == null) return;

        if (_mainCamTransform == null && Camera.main != null)
        {
            _mainCamTransform = Camera.main.transform;
        }

        string extraText = isCrit ? "!" : "";
        _textMesh.text = Mathf.CeilToInt(damageAmount).ToString() + extraText;

        if (visualConfig != null)
        {
            visualConfig.ResolveStyle(type, isCrit, isPlayer, isBleed, out _textColor, out float fontSize);
            _textMesh.fontSize = fontSize;
        }
        else
        {
            ResolveFallbackStyle(type, isCrit, isPlayer, isBleed, out _textColor, out float fontSize);
            _textMesh.fontSize = fontSize;
        }

        _textColor.a = 1f;
        _textMesh.color = _textColor;

        _disappearTimer = 0.5f;
        _moveVector = new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f)) * 2f;
    }

    private static void ResolveFallbackStyle(DamageType type, bool isCrit, bool isPlayer, bool isBleed, out Color color, out float fontSize)  //el SO es para poder borrar esto o estoy mal?
    {
        color = new Color(0.9f, 0.9f, 0.9f, 1f);
        switch (type)
        {
            case DamageType.Fuego: color = new Color(1f, 0.4f, 0f, 1f); break;
            case DamageType.Agua: color = new Color(0f, 0.6f, 1f, 1f); break;
            case DamageType.Electrico: color = Color.yellow; break;
            case DamageType.Veneno: color = new Color(0.7f, 0f, 1f, 1f); break;
            case DamageType.Magico: color = Color.magenta; break;
        }

        if (isPlayer) color = Color.red;
        if (isBleed) color = new Color(0.8f, 0f, 0f, 1f);

        if (isPlayer) fontSize = 12f;
        else if (isCrit) fontSize = 14f;
        else if (isBleed) fontSize = 8f;
        else fontSize = 10f;
    }

    private void Update()
    {
        if (_textMesh == null)
        {
            ReturnOrDestroy();
            return;
        }

        if (_mainCamTransform != null)
        {
            transform.rotation = _mainCamTransform.rotation;
        }
        else if (Camera.main != null)
        {
            _mainCamTransform = Camera.main.transform;
            transform.rotation = _mainCamTransform.rotation;
        }

        transform.position += _moveVector * Time.deltaTime;
        _moveVector -= _moveVector * (8f * Time.deltaTime);

        _disappearTimer -= Time.deltaTime;
        if (_disappearTimer < 0f)
        {
            const float fadeAmount = 5f;
            _textColor.a -= fadeAmount * Time.deltaTime;
            _textMesh.color = _textColor;

            if (_textColor.a <= 0f)
            {
                ReturnOrDestroy();
            }
        }
    }

    private void ReturnOrDestroy()
    {
        if (VfxPoolManager.Instance != null)
        {
            VfxPoolManager.Instance.ReturnVfx(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}