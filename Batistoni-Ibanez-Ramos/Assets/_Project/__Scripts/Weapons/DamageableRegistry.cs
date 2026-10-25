using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public static class DamageableRegistry
    {
        public readonly struct Entry
        {
            public readonly IDamageable Damageable;
            public readonly Transform RootTransform;

            public Entry(IDamageable damageable, Transform rootTransform)
            {
                Damageable = damageable;
                RootTransform = rootTransform;
            }
        }

        private static readonly Dictionary<int, Entry> _colliderCache = new Dictionary<int, Entry>(512);

        public static void Register(Collider col, IDamageable damageable, Transform rootTransform = null)
        {
            if (col == null || damageable == null) return;

            Transform root = rootTransform != null
                ? rootTransform
                : (damageable is Component comp ? comp.transform : col.transform);

            _colliderCache[col.GetInstanceID()] = new Entry(damageable, root);
        }

        public static void Unregister(Collider col)
        {
            if (col == null) return;
            _colliderCache.Remove(col.GetInstanceID());
        }

        public static bool TryGet(Collider col, out IDamageable damageable, out Transform rootTransform)
        {
            damageable = null;
            rootTransform = null;

            if (col == null) return false;

            int id = col.GetInstanceID();

            if (_colliderCache.TryGetValue(id, out Entry cached))
            {
                if (cached.RootTransform != null)
                {
                    damageable = cached.Damageable;
                    rootTransform = cached.RootTransform;
                    return true;
                }
                _colliderCache.Remove(id);
            }

            if (!col.TryGetComponent(out damageable))
            {
                damageable = col.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                rootTransform = (damageable is Component comp) ? comp.transform : col.transform;
                _colliderCache[id] = new Entry(damageable, rootTransform);
                return true;
            }

            return false;
        }

        public static void Clear()
        {
            _colliderCache.Clear();
        }
    }
}