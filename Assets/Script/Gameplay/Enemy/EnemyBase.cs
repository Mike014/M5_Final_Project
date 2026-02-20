using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,      // Stationary: si gira / Patrol: cammina sui waypoint
    Chase,     // Insegue il player
    Search,    // Cerca nell'area dell'ultima posizione nota
    Return,    // Torna alla posizione/percorso originale
    Stunned    // Il player spara un "colpo stordente", se colpisce un nemico questo poi torna al suo stato precedente.
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

    [Header("Search")]
    [SerializeField] private float _searchDuration = 5f;   // Secondi prima di arrendersi
    [SerializeField] private float _searchRadius = 5f;     // Raggio punti casuali di ricerca
    [SerializeField] private float _alertRadius = 20f;     // Raggio allerta nemici vicini

    [Header("Stun")]
    [SerializeField] private float _stunDuration = 3f;
    private float _stunTimer;
    private EnemyState _stateBeforeStun; // Ricorda lo stato precedente

    protected NavMeshAgent _agent;
    protected EnemyState _currentState;
    protected Vector3 _lastKnownPlayerPosition;

    private float _searchTimer;
    private bool _isSearching = false;
    private bool _hasAlerted = false; // Evita di chiamare AlertNearbyEnemies ogni frame

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _currentState = EnemyState.Idle;
    }

    protected virtual void Update()
    {
        if (_currentState == EnemyState.Stunned)
        {
            HandleStunned();
            return;
        }
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
            case EnemyState.Search: HandleSearch(); break;
            case EnemyState.Return: HandleReturn(); break;
            case EnemyState.Stunned: HandleStunned(); break;
        }
    }

    protected abstract void HandleIdle();

    protected virtual void HandleChase()
    {
        _agent.speed = _chaseSpeed;

        if (CanSeePlayer())
        {
            _lastKnownPlayerPosition = _player.position;
            _agent.SetDestination(_player.position);

            // Allerta i vicini solo una volta per ogni ingresso in Chase
            // _hasAlerted si resetta in SetState quando si esce da Chase
            if (!_hasAlerted)
            {
                AlertNearbyEnemies();
                _hasAlerted = true;
            }
        }
        else
        {
            // Player perso → vai in Search invece di Return direttamente
            SetState(EnemyState.Search);
        }
    }

    protected virtual void HandleSearch()
    {
        _searchTimer += Time.deltaTime;

        // Timer scaduto → rinuncia e torna alla base
        if (_searchTimer >= _searchDuration)
        {
            _searchTimer = 0f;
            _isSearching = false;
            Debug.Log($"{gameObject.name}: Ricerca fallita → Return");
            SetState(EnemyState.Return);
            return;
        }

        // Ha ritrovato il player durante la ricerca → Chase
        if (CanSeePlayer())
        {
            _searchTimer = 0f;
            _isSearching = false;
            SetState(EnemyState.Chase);
            return;
        }

        // Genera un nuovo punto casuale quando arriva al precedente
        if (!_isSearching ||
            (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance))
        {
            Vector3 randomPoint = _lastKnownPlayerPosition +
                                  Random.insideUnitSphere * _searchRadius;
            randomPoint.y = transform.position.y;

            // SamplePosition trova il punto valido più vicino sulla NavMesh
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, _searchRadius, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
                _isSearching = true;
                Debug.Log($"{gameObject.name}: Cerco in {hit.position}");
            }
        }
    }

    protected abstract void HandleReturn();

    protected void SetState(EnemyState newState)
    {
        if (_currentState == newState) return;

        // Reset flag allerta quando si esce da Chase
        if (_currentState == EnemyState.Chase)
            _hasAlerted = false;

        _currentState = newState;
        _agent.speed = _normalSpeed;
    }

    // Avvisa tutti i nemici nel raggio _alertRadius
    protected void AlertNearbyEnemies()
    {
        // Log la distanza da tutti i nemici nella scena
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
        foreach (EnemyBase e in allEnemies)
        {
            if (e != this)
            {
                float dist = Vector3.Distance(transform.position, e.transform.position);
                Debug.Log($"{gameObject.name} → {e.gameObject.name}: distanza {dist:F2}");
            }
        }
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, _alertRadius);
        Debug.Log($"{gameObject.name}: OverlapSphere trovato {nearbyColliders.Length} collider nel raggio {_alertRadius}");

        foreach (Collider col in nearbyColliders)
        {
            EnemyBase enemy = col.GetComponent<EnemyBase>();
            Debug.Log($"  → Collider: {col.gameObject.name} | EnemyBase: {(enemy != null ? "trovato" : "NULL")}");

            if (enemy != null && enemy != this)
            {
                enemy.ReceiveAlert(_lastKnownPlayerPosition);
            }
        }
    }

    // Riceve l'allerta da un altro nemico
    public void ReceiveAlert(Vector3 playerPosition)
    {
        if (_currentState == EnemyState.Idle || _currentState == EnemyState.Return)
        {
            _lastKnownPlayerPosition = playerPosition;
            SetState(EnemyState.Chase);
            Debug.Log($"{gameObject.name}: Allertato! Vado in Chase");
        }
    }

    protected bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector3 dirToPlayer = _player.position - transform.position;
        float distance = dirToPlayer.magnitude;

        if (distance > _visionRange) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > _visionAngle / 2f) return false;

        if (Physics.Raycast(transform.position + Vector3.up,
                            dirToPlayer.normalized,
                            distance,
                            LayerMask.GetMask("Wall")))
            return false;

        return true;
    }

    public void GetStunned()
    {
        if (_currentState == EnemyState.Stunned) return; // Evita doppio stun

        _stateBeforeStun = _currentState;
        _agent.ResetPath();
        _agent.isStopped = true;

        // Assegna direttamente senza passare per SetState
        // (SetState resetta la speed e potrebbe causare problemi)
        _currentState = EnemyState.Stunned;

        Debug.Log($"{gameObject.name}: STORDITO | Stato salvato: {_stateBeforeStun}");
    }
    private void HandleStunned()
    {
        _stunTimer += Time.deltaTime;

        if (_stunTimer >= _stunDuration)
        {
            _stunTimer = 0f;
            _agent.isStopped = false;

            // Torna allo stato precedente allo stun
            SetState(_stateBeforeStun);
            Debug.Log($"{gameObject.name}: Stun terminato → torno a {_stateBeforeStun}");
        }
    }

    private void OnDrawGizmos()
    {
        // Colore in base allo stato
        Gizmos.color = _currentState switch
        {
            EnemyState.Chase => Color.red,
            EnemyState.Search => Color.blue,
            EnemyState.Stunned => Color.cyan,
            _ => Color.yellow
        };

        Gizmos.DrawWireSphere(transform.position, _visionRange);

        Vector3 rightBoundary = Quaternion.Euler(0, _visionAngle / 2f, 0) * transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -_visionAngle / 2f, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, rightBoundary * _visionRange);
        Gizmos.DrawRay(transform.position, leftBoundary * _visionRange);
    }
}
