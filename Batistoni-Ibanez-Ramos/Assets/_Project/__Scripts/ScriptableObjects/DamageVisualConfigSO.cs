using System;
using System.Collections.Generic;
using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    [CreateAssetMenu(fileName = "DamageVisualConfig", menuName = "Config/Damage Visual Config")]
    public class DamageVisualConfigSO : ScriptableObject
    {
        [Serializable]
        public struct DamageTypeColorEntry
        {
            public DamageType damageType;
            public Color color;
        }

        [Header("Paleta de Colores por Tipo de Daño")]
        [SerializeField]
        private List<DamageTypeColorEntry> typeColors = new List<DamageTypeColorEntry>
        {
            new DamageTypeColorEntry { damageType = DamageType.Fisico, color = new Color(0.9f, 0.9f, 0.9f, 1f) },
            new DamageTypeColorEntry { damageType = DamageType.Fuego, color = new Color(1f, 0.4f, 0f, 1f) },
            new DamageTypeColorEntry { damageType = DamageType.Agua, color = new Color(0f, 0.6f, 1f, 1f) },
            new DamageTypeColorEntry { damageType = DamageType.Electrico, color = Color.yellow },
            new DamageTypeColorEntry { damageType = DamageType.Veneno, color = new Color(0.7f, 0f, 1f, 1f) },
            new DamageTypeColorEntry { damageType = DamageType.Magico, color = Color.magenta }
        };

        [Header("Colores Especiales")]
        public Color playerDamageColor = Color.red;
        public Color bleedDamageColor = new Color(0.8f, 0f, 0f, 1f);
        public Color defaultFallbackColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        [Header("Tamaños de Fuente")]
        public float normalFontSize = 10f;
        public float critFontSize = 14f;
        public float playerDamageFontSize = 12f;
        public float bleedFontSize = 8f;

        private readonly Dictionary<int, Color> _colorLookup = new Dictionary<int, Color>(8);
        private bool _isLookupBuilt;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void OnValidate()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _colorLookup.Clear();
            if (typeColors != null)
            {
                for (int i = 0; i < typeColors.Count; i++)
                {
                    _colorLookup[(int)typeColors[i].damageType] = typeColors[i].color;
                }
            }
            _isLookupBuilt = true;
        }

        public void ResolveStyle(DamageType type, bool isCrit, bool isPlayer, bool isBleed, out Color color, out float fontSize)
        {
            if (!_isLookupBuilt) BuildLookup();

            if (isPlayer)
            {
                color = playerDamageColor;
                fontSize = playerDamageFontSize;
                return;
            }

            if (isBleed)
            {
                color = bleedDamageColor;
                fontSize = bleedFontSize;
                return;
            }

            if (!_colorLookup.TryGetValue((int)type, out color))
            {
                color = defaultFallbackColor;
            }

            color.a = 1f;
            fontSize = isCrit ? critFontSize : normalFontSize;
        }
    }
}