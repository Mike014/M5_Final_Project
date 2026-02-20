using System.Threading.Tasks.Sources;
using UnityEngine;
using UnityEngine.AI;

// Ogni nemico può essere in UNO SOLO di questi stati
public enum EnemyState
{
    Idle,      // Stationary: si gira / Patrol: cammina sui waypoint
    Chase,     // Insegue il player
    Return     // Torna alla posizione/percorso originale
}

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] protected float _visionAngle = 60f;
    [SerializeField] protected float _visionRange = 10f;
    [SerializeField] protected Transform _player;

    [Header("Chase")]
    [SerializeField] protected float _chaseSpeed = 5f;
    [SerializeField] protected float _normalSpeed = 2f;

    protected NavMeshAgent _agent;
    protected EnemyState _currentState;
    protected Vector3 _lastKnownPlayerPosition;

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _currentState = EnemyState.Idle;
    }

    protected virtual void Update()
    {
        // Controlla visione ogni frame
        if (CanSeePlayer())
        {
            _lastKnownPlayerPosition = _player.position;
            SetState(EnemyState.Chase);
        }

        // Esegue il comportamento dello stato corrente
        switch (_currentState)
        {
            case EnemyState.Idle: HandleIdle(); break;
            case EnemyState.Chase: HandleChase(); break;
            case EnemyState.Return: HandleReturn(); break;
        }
    }

    // Ogni nemico implementa il suo Idle a modo suo
    protected abstract void HandleIdle();

    // In EnemyBase.cs — sostituisci HandleChase()
    protected virtual void HandleChase()
    {
        _agent.speed = _chaseSpeed;

        if (CanSeePlayer())
        {
            _lastKnownPlayerPosition = _player.position;
            _agent.SetDestination(_player.position);
        }
        else
        {
            _agent.SetDestination(_lastKnownPlayerPosition);

            if (!_agent.pathPending &&
                _agent.remainingDistance <= _agent.stoppingDistance)
            {
                SetState(EnemyState.Return);
            }
        }
    }

    protected abstract void HandleReturn();

    protected void SetState(EnemyState newState)
    {
        if (_currentState == newState) return;
        _currentState = newState;
        _agent.speed = _normalSpeed;
    }

    protected bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector3 dirToPlayer = _player.position - transform.position;
        float distance = dirToPlayer.magnitude;

        // 1. Il player è nel range?
        if (distance > _visionRange) return false;

        // 2. Il player è nell'angolo del cono?
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > _visionAngle / 2f) return false;

        // 3. C'è un muro in mezzo? (Raycast per line of sight)
        if (Physics.Raycast(transform.position + Vector3.up,
                            dirToPlayer.normalized,
                            distance,
                            LayerMask.GetMask("Wall")))
            return false;

        return true;
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         Debug.Log($"{gameObject.name}: Player catturato!");
    //         GameController.Instance.OnPlayerCaught();
    //     }
    // }

    // OnDrawGizmos viene chiamato automaticamente dall'Editor ogni frame
    private void OnDrawGizmos()
    {
        // Colore diverso in base allo stato
        Gizmos.color = _currentState == EnemyState.Chase
            ? Color.red    // Sta inseguendo
            : Color.yellow; // Idle o Return

        // Disegna il range di visione come sfera wireframe
        Gizmos.DrawWireSphere(transform.position, _visionRange);

        // Calcola i due raggi laterali del cono
        // Ruota il forward di +/- metà dell'angolo
        Vector3 rightBoundary = Quaternion.Euler(0, _visionAngle / 2f, 0)
                                * transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -_visionAngle / 2f, 0)
                                * transform.forward;

        // Disegna le due linee laterali del cono
        Gizmos.DrawRay(transform.position, rightBoundary * _visionRange);
        Gizmos.DrawRay(transform.position, leftBoundary * _visionRange);
    }
}
