using UnityEngine;

namespace PatronesAplicados
{
    public class EnemyFlashController
    {
        private Renderer[] _renderers;
        private Material[] _originalMaterials;
        private float _flashTimer;
        private bool _isFlashing;

        public void Initialize(Component root)
        {
            if (root == null) return;

            _renderers = root.GetComponentsInChildren<Renderer>();
            if (_renderers != null && _renderers.Length > 0)
            {
                _originalMaterials = new Material[_renderers.Length];
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] != null)
                    {
                        _originalMaterials[i] = _renderers[i].sharedMaterial;
                    }
                }
            }
        }

        public void TriggerFlash(Material flashMaterial, float duration = 0.1f)
        {
            if (flashMaterial == null || _renderers == null) return;

            _flashTimer = duration;
            if (_isFlashing) return;

            _isFlashing = true;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _renderers[i].sharedMaterial = flashMaterial;
                }
            }
        }

        public void Tick(float deltaTime)
        {
            if (!_isFlashing) return;

            _flashTimer -= deltaTime;
            if (_flashTimer <= 0f)
            {
                RestoreOriginalMaterials();
            }
        }

        public void RestoreOriginalMaterials()
        {
            _isFlashing = false;
            _flashTimer = 0f;

            if (_renderers == null || _originalMaterials == null) return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _originalMaterials[i] != null)
                {
                    _renderers[i].sharedMaterial = _originalMaterials[i];
                }
            }
        }
    }
}