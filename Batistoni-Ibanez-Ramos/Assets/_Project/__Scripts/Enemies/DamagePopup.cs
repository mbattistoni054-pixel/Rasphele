using UnityEngine;
using TMPro; 

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(float damageAmount, DamageType type, bool isCrit = false, bool isPlayer = false, bool isBleed = false)
    {
        string extraText = isCrit ? "!" : "";
        textMesh.text = Mathf.CeilToInt(damageAmount).ToString() + extraText;

        //  COLORES POR TIPO DE ELEMENTO
        switch (type)
        {
            case DamageType.Fisico: textColor = new Color(0.9f, 0.9f, 0.9f); break; // Blanco Suave
            case DamageType.Fuego: textColor = new Color(1f, 0.4f, 0f); break;      // Naranja
            case DamageType.Agua: textColor = new Color(0f, 0.6f, 1f); break;       // Azul Claro
            case DamageType.Electrico: textColor = Color.yellow; break;             // Amarillo
            case DamageType.Veneno: textColor = new Color(0.7f, 0f, 1f); break;     // Morado
            case DamageType.Magico: textColor = Color.magenta; break;               // Rosa
        }

        // Sobreescrituras especiales para el Jugador o el Sangrado
        if (isPlayer) textColor = Color.red;
        if (isBleed) textColor = new Color(0.8f, 0f, 0f); // Rojo oscuro

        textMesh.color = textColor;

        // TAMAÑOS 
        if (isPlayer) textMesh.fontSize = 12;
        else if (isCrit) textMesh.fontSize = 14;
        else if (isBleed) textMesh.fontSize = 8; // Efectos de estado un poco más chicos
        else textMesh.fontSize = 10;

        disappearTimer = 0.5f;

        // MOVIMIENTO
        moveVector = new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f)) * 2f;
    }

    private void Update()
    {
        // Fuerza al texto a mirar siempre a la cámara
        transform.rotation = Camera.main.transform.rotation;

        // Animar el movimiento hacia arriba
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime; // Fricción suave

        // Desaparecer gradualmente
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float fadeAmount = 5f;
            textColor.a -= fadeAmount * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}