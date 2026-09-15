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

        switch (type)
        {
            case DamageType.Fisico: textColor = new Color(0.9f, 0.9f, 0.9f); break; 
            case DamageType.Fuego: textColor = new Color(1f, 0.4f, 0f); break;     
            case DamageType.Agua: textColor = new Color(0f, 0.6f, 1f); break;       
            case DamageType.Electrico: textColor = Color.yellow; break;             
            case DamageType.Veneno: textColor = new Color(0.7f, 0f, 1f); break;     
            case DamageType.Magico: textColor = Color.magenta; break;               
        }

        if (isPlayer) textColor = Color.red;
        if (isBleed) textColor = new Color(0.8f, 0f, 0f);

        textMesh.color = textColor;

        if (isPlayer) textMesh.fontSize = 12;
        else if (isCrit) textMesh.fontSize = 14;
        else if (isBleed) textMesh.fontSize = 8; 
        else textMesh.fontSize = 10;

        disappearTimer = 0.5f;

        moveVector = new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f)) * 2f;
    }

    private void Update()
    {
        transform.rotation = Camera.main.transform.rotation;

        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime; 

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