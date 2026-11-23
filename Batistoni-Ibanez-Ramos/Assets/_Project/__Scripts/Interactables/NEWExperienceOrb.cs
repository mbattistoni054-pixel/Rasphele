using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWExperienceOrb : MonoBehaviour
{
    [SerializeField] private XpOrbData data;

    [Header("Ajustes de la Gema")]
    public float xpAmount = 20f;

    private float _defaultXpAmount;
    private float _currentMoveSpeed;
    private Transform _player;
    private bool _isAttracted;
    private bool _isCollected;

    private void Awake()
    {
        _defaultXpAmount = xpAmount;
    }

    private void OnEnable()
    {
        _isAttracted = false;
        _isCollected = false;
        _currentMoveSpeed = data.BaseMoveSpeed;
        xpAmount = _defaultXpAmount;

        if (GameManagerRefactored.Instance != null && GameManagerRefactored.Instance.Player != null)
        {
            _player = GameManagerRefactored.Instance.Player;
        }
    }

    public void SetXpAmount(float amount)
    {
        if (amount > 0f)
        {
            xpAmount = amount;
        }
    }

    private void Update()
    {
        if (PauseManager.IsPaused || _isCollected || _player == null) return;

        if (!_isAttracted)
        {
            float sqrDist = (_player.position - transform.position).sqrMagnitude;
            if (sqrDist <= data.MagnetRadius * data.MagnetRadius)
            {
                _isAttracted = true;
            }
        }

        if (_isAttracted)
        {
            transform.position = Vector3.MoveTowards(transform.position, _player.position, _currentMoveSpeed * Time.deltaTime);
        }
    }

    public void ForceAttract(Transform targetPlayer)
    {
        _player = targetPlayer;
        _isAttracted = true;
        _currentMoveSpeed = data.BaseMoveSpeed * 2f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isCollected) return;

        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out PlayerExperienceRefactored newXp))
            {
                _isCollected = true;
                newXp.AddExperience(xpAmount);
                ReturnToPool();
            }
        }
    }

    private void ReturnToPool()
    {
        if (ProjectilePoolManager.Instance != null)
        {
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}