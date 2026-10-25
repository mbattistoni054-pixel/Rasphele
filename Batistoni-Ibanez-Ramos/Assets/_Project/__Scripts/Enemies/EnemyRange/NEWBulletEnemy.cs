using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWBulletEnemy : MonoBehaviour, IPausable
{
    private Transform _player;
    public float damage;
    public float offset;
    [SerializeField] private float speed = 80f;

    private float _lifeTimer;
    [SerializeField] private float maxLifeTime = 4f;
    private bool _isReturned;
    private bool _isPaused;

    private void OnEnable()
    {
        _lifeTimer = 0f;
        _isReturned = false;
        _isPaused = PauseManager.IsPaused;
        PauseManager.Register(this);

        _player = GameManagerRefactored.Instance != null ? GameManagerRefactored.Instance.Player : null;
        if (_player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        AimAtTarget();
    }

    private void OnDisable()
    {
        PauseManager.Unregister(this);
    }

    public void Setup(float bulletDamage, Transform targetPlayer = null)
    {
        damage = bulletDamage;
        if (targetPlayer != null)
        {
            _player = targetPlayer;
        }
        AimAtTarget();
    }

    private void AimAtTarget()
    {
        if (_player == null) return;

        Vector3 targetPoint = new Vector3(_player.position.x, _player.position.y + offset, _player.position.z);
        Vector3 direction = (targetPoint - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.up = direction;
        }
    }

    private void Update()
    {
        if (_isReturned || _isPaused) return;

        transform.position += transform.up * (speed * Time.deltaTime);

        _lifeTimer += Time.deltaTime;
        if (_lifeTimer >= maxLifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isReturned || _isPaused) return;

        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damage);
            }

            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (_isReturned) return;
        _isReturned = true;

        if (ProjectilePoolManager.Instance != null)
        {
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnPause() => _isPaused = true;
    public void OnResume() => _isPaused = false;
}