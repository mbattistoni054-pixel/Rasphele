using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados
{
    public class WeaponUpgradeProfile
    {
        public Dictionary<UpgradeData, int> levels = new Dictionary<UpgradeData, int>();
        public List<Sprite> firingIcons = new List<Sprite>();
        public List<Sprite> impactIcons = new List<Sprite>();
    }
}
