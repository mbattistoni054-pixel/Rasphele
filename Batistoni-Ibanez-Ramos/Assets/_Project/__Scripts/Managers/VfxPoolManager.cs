using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class VfxPoolManager : MonoBehaviour
    {
        public static VfxPoolManager Instance { get; private set; }

        private readonly Dictionary<int, Queue<GameObject>> _pool = new Dictionary<int, Queue<GameObject>>(32);
        private readonly Dictionary<GameObject, int> _instanceToPrefabId = new Dictionary<GameObject, int>(256);
        private readonly HashSet<GameObject> _pooledObjects = new HashSet<GameObject>(256);
        private readonly Dictionary<GameObject, ParticleSystem> _particleCache = new Dictionary<GameObject, ParticleSystem>(128);
        private readonly Dictionary<GameObject, DamagePopup> _popupCache = new Dictionary<GameObject, DamagePopup>(128);
        private readonly Dictionary<GameObject, PooledExplosionVisual> _autoReturnCache = new Dictionary<GameObject, PooledExplosionVisual>(128);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public GameObject GetVfx(GameObject prefab, Vector3 position, Quaternion rotation, float autoReturnTime = 0f)
        {
            if (prefab == null) return null;

            int prefabId = prefab.GetInstanceID();

            if (!_pool.TryGetValue(prefabId, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>(32);
                _pool.Add(prefabId, queue);
            }

            GameObject vfxObj = null;

            while (queue.Count > 0 && vfxObj == null)
            {
                GameObject candidate = queue.Dequeue();
                if (candidate != null)
                {
                    _pooledObjects.Remove(candidate);
                    vfxObj = candidate;
                }
            }

            if (vfxObj != null)
            {
                vfxObj.transform.SetPositionAndRotation(position, rotation);
                vfxObj.SetActive(true);
            }
            else
            {
                vfxObj = Instantiate(prefab, position, rotation, transform);
                _instanceToPrefabId[vfxObj] = prefabId;

                ParticleSystem ps = vfxObj.GetComponentInChildren<ParticleSystem>();
                if (ps != null) _particleCache[vfxObj] = ps;

                if (vfxObj.TryGetComponent(out PooledExplosionVisual autoRet))
                {
                    _autoReturnCache[vfxObj] = autoRet;
                }
                else if (autoReturnTime > 0f)
                {
                    autoRet = vfxObj.AddComponent<PooledExplosionVisual>();
                    autoRet.lifetime = autoReturnTime;
                    _autoReturnCache[vfxObj] = autoRet;
                }
            }

            if (autoReturnTime > 0f && _autoReturnCache.TryGetValue(vfxObj, out PooledExplosionVisual cachedAutoRet) && cachedAutoRet != null)
            {
                cachedAutoRet.lifetime = autoReturnTime;
            }

            if (_particleCache.TryGetValue(vfxObj, out ParticleSystem cachedPs) && cachedPs != null)
            {
                cachedPs.Clear();
                cachedPs.Play();
            }

            return vfxObj;
        }

        public DamagePopup SpawnDamagePopup(
            GameObject popupPrefab,
            Vector3 position,
            float damageAmount,
            DamageType damageType,
            bool isCrit = false,
            bool isPlayer = false,
            bool isBleed = false)
        {
            GameObject obj = GetVfx(popupPrefab, position, Quaternion.identity);
            if (obj == null) return null;

            if (!_popupCache.TryGetValue(obj, out DamagePopup popup) || popup == null)
            {
                if (obj.TryGetComponent(out popup))
                {
                    _popupCache[obj] = popup;
                }
            }

            if (popup != null)
            {
                popup.Setup(damageAmount, damageType, isCrit, isPlayer, isBleed);
            }

            return popup;
        }

        public void ReturnVfx(GameObject vfxObj)
        {
            if (vfxObj == null) return;
            if (!_pooledObjects.Add(vfxObj)) return;

            vfxObj.SetActive(false);

            if (!_instanceToPrefabId.TryGetValue(vfxObj, out int prefabId))
            {
                prefabId = vfxObj.GetInstanceID();
                _instanceToPrefabId[vfxObj] = prefabId;
            }

            if (!_pool.TryGetValue(prefabId, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>(32);
                _pool.Add(prefabId, queue);
            }

            queue.Enqueue(vfxObj);
        }
    }
}