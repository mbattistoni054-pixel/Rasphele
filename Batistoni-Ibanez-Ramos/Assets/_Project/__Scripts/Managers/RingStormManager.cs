using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PatronesAplicados;

[System.Serializable]
public class StormPhase
{
    public string phaseName = "Fase 1 (0s - 60s)";
    public float startTimeInSeconds;
    public float endTimeInSeconds;

    [Tooltip("Velocidad de cierre (Grados por segundo). Ej: 1 significa que cierra 1 grado por segundo. 0 significa que se queda quieta.")]
    public float closingSpeed = 0f;
}

public class RingStormManager : MonoBehaviour, IPausable
{
    [Header("Referencias del Mapa")]
    [Tooltip("El Empty colocado exactamente en el centro del anillo. Su eje Z (Azul) marca dónde empieza la tormenta.")]
    public Transform mapCenter;
    public Transform player;

    [Header("Muros Visuales")]
    [Tooltip("La pared que se queda quieta en el punto de inicio (Ángulo 0).")]
    public Transform staticWall;
    [Tooltip("La pared que se mueve y va barriendo el mapa.")]
    public Transform movingWall;
    public Transform movingWall2;

    [Header("Configuración por Fases (Oleadas)")]
    public List<StormPhase> phases = new List<StormPhase>();

    [Tooltip("Ángulo libre que quedará al final. Ej: Si pones 45, dejará un hueco de 45 grados.")]
    public float minimumSafeAngle = 0f;
    [Tooltip("¿La tormenta avanza hacia la derecha (reloj) o izquierda?")]
    public bool sweepClockwise = true;

    [Header("Filtro de Pantalla y Daño")]
    public float damagePerSecond = 5f;
    public Image stormScreenFilter;
    public float filterFadeSpeed = 2f;

    private float _levelTimer;
    private float _currentStormAngle;

    public bool isInStorm;
    private float _damageTimer;
    private bool _isPaused;
    private bool _wasFilterActive;

    public float rotationPerSec;

    private IDamageable _cachedPlayerDamageable;

    private void OnEnable()
    {
        _isPaused = PauseManager.IsPaused;
        PauseManager.Register(this);
    }

    private void OnDisable()
    {
        PauseManager.Unregister(this);
    }

    private void Start()
    {
        _levelTimer = 0f;

        if (player == null)
        {
            player = GameManagerRefactored.Instance != null ? GameManagerRefactored.Instance.Player : null;
            if (player == null)
            {
                GameObject pObj = GameObject.FindGameObjectWithTag("Player");
                if (pObj != null) player = pObj.transform;
            }
        }

        if (player != null)
        {
            player.TryGetComponent(out _cachedPlayerDamageable);
        }

        if (stormScreenFilter != null)
        {
            stormScreenFilter.gameObject.SetActive(false);
            _wasFilterActive = false;
        }
    }

    private void Update()
    {
        if (_isPaused || mapCenter == null || player == null) return;

        float dt = Time.deltaTime;
        _levelTimer += dt;
        StormPhase currentPhase = GetCurrentPhase();

        if (currentPhase != null)
        {
            float maxStormAngle = 360f - minimumSafeAngle;
            if (_currentStormAngle < maxStormAngle)
            {
                _currentStormAngle = Mathf.Min(maxStormAngle, _currentStormAngle + currentPhase.closingSpeed * dt);
            }
        }

        if (movingWall != null && movingWall2 != null)
        {
            if (movingWall2.eulerAngles.y < 204f)
            {
                float rotStep = rotationPerSec * dt;
                movingWall.Rotate(0f, rotStep, 0f);
                movingWall2.Rotate(0f, rotStep, 0f);
            }
        }

        HandleStormEffects(dt);
    }

    private StormPhase GetCurrentPhase()
    {
        if (phases == null || phases.Count == 0) return null;

        for (int i = 0; i < phases.Count; i++)
        {
            StormPhase phase = phases[i];
            if (_levelTimer >= phase.startTimeInSeconds && _levelTimer < phase.endTimeInSeconds)
            {
                return phase;
            }
        }

        return phases[phases.Count - 1];
    }

    private void HandleStormEffects(float dt)
    {
        if (stormScreenFilter != null && _wasFilterActive != isInStorm)
        {
            stormScreenFilter.gameObject.SetActive(isInStorm);
            _wasFilterActive = isInStorm;
        }

        if (isInStorm)
        {
            _damageTimer += dt;
            if (_damageTimer >= 1f)
            {
                _damageTimer -= 1f;

                if (_cachedPlayerDamageable == null && player != null)
                {
                    player.TryGetComponent(out _cachedPlayerDamageable);
                }

                _cachedPlayerDamageable?.TakeDamage(damagePerSecond);
            }
        }
        else
        {
            _damageTimer = 0f;
        }
    }

    public void OnPause() => _isPaused = true;
    public void OnResume() => _isPaused = false;
}