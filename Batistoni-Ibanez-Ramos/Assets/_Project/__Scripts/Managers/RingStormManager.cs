using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class StormPhase
{
    public string phaseName = "Fase 1 (0s - 60s)";
    public float startTimeInSeconds;
    public float endTimeInSeconds;

    [Tooltip("Velocidad de cierre (Grados por segundo). Ej: 1 significa que cierra 1 grado por segundo. 0 significa que se queda quieta.")]
    public float closingSpeed = 0f;
}

public class RingStormManager : MonoBehaviour
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

    private float levelTimer = 0f;
    private float currentStormAngle = 0f; 

    public bool isInStorm = false;
    private float damageTimer = 0f;

    public float rotationPerSec;

    void Start()
    {
        levelTimer = 0f;

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }
    }

    void Update()
    {
        if (mapCenter == null || player == null) return;

        levelTimer += Time.deltaTime;
        StormPhase currentPhase = GetCurrentPhase();

        if (currentPhase != null)
        {
            float maxStormAngle = 360f - minimumSafeAngle;
            if (currentStormAngle < maxStormAngle)
            {
                currentStormAngle += currentPhase.closingSpeed * Time.deltaTime;
                if (currentStormAngle > maxStormAngle) currentStormAngle = maxStormAngle;
            }
        }

        if (movingWall != null)
        {
           
            float direction = sweepClockwise ? currentStormAngle : -currentStormAngle;

            if (movingWall2.eulerAngles.y < 204)
            {
                movingWall.Rotate(0f, rotationPerSec * Time.deltaTime, 0f);
                movingWall2.Rotate(0f, rotationPerSec * Time.deltaTime, 0f);
            }

        }

        HandleStormEffects();
    }

    private StormPhase GetCurrentPhase()
    {
        if (phases.Count == 0) return null;

        foreach (StormPhase phase in phases)
        {
            if (levelTimer >= phase.startTimeInSeconds && levelTimer < phase.endTimeInSeconds)
            {
                return phase;
            }
        }

        return phases[phases.Count - 1];
    }

    private void HandleStormEffects()
    {
      

        if (isInStorm)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= 1f)
            {
                damageTimer -= 1f;

                var refHealth = player.GetComponent<PatronesAplicados.PlayerHealthRefactored>();
                if (refHealth != null)
                {
                    refHealth.TakeDamage(damagePerSecond);
                }
            }
            stormScreenFilter.gameObject.SetActive(true);
        }
        else
        {
            damageTimer = 0f;
            stormScreenFilter.gameObject.SetActive(false);
        }
    }

}
