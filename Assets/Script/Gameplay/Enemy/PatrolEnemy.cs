using UnityEngine;

public class PatrolEnemy : EnemyBase
{
    [Header("Patrol")]
    [SerializeField] private Transform[] _waypoints;

    private int _currentWaypointIndex = 0;
    private int _lastWaypointIndex;

    protected override void Awake()
    {
        base.Awake();

        // CHECK DI SICUREZZA: Evitiamo crash se dimentichi di assegnare i waypoint
        if (_waypoints == null || _waypoints.Length == 0)
        {
            Debug.LogWarning($"PatrolEnemy: [{gameObject.name}] ATTENZIONE! Nessun waypoint assegnato nell'Inspector!");
        }
        else
        {
            Debug.Log($"PatrolEnemy: [{gameObject.name}] Awake. Assegnati {_waypoints.Length} waypoint per la pattuglia.");
        }
    }

    protected override void HandleIdle()
    {
        if (_waypoints.Length == 0) return;

        if (!_agent.pathPending &&
            _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _lastWaypointIndex = _currentWaypointIndex;
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;

            // LOG DI AZIONE: Stampiamo verso quale waypoint stiamo andando
            Debug.Log($"PatrolEnemy: [{gameObject.name}] Raggiunto waypoint {_lastWaypointIndex}. Mi dirigo verso il waypoint {_currentWaypointIndex}.");

            _agent.SetDestination(_waypoints[_currentWaypointIndex].position);
        }
    }

    protected override void HandleReturn()
    {
        // LOG SPIA: Messo in un blocco per non spammare la console a 60 FPS
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"PatrolEnemy: [{gameObject.name}] RETURN verso waypoint {_lastWaypointIndex}. Distanza: {_agent.remainingDistance}");
        }

        _agent.SetDestination(_waypoints[_lastWaypointIndex].position);

        if (!_agent.pathPending &&
            _agent.remainingDistance <= _agent.stoppingDistance)
        {
            // LOG DI TRANSIZIONE: Conferma del rientro nei ranghi
            Debug.Log($"PatrolEnemy: [{gameObject.name}] Tornato al percorso di pattuglia. Riprendo l'Idle (Patrol).");
            SetState(EnemyState.Idle);
        }
    }

    protected override void HandleChase()
    {
        _agent.speed = _chaseSpeed;

        // SCELTA A: Lo vedo ANCORA
        if (CanSeePlayer())
        {
            // Continuo ad inseguirlo attivamente
            _agent.SetDestination(_player.position);
        }
        // SCELTA B: L'ho perso di vista!
        else
        {
            // Vado a controllare l'ultimo punto in cui l'ho visto
            _agent.SetDestination(_lastKnownPlayerPosition);

            // ATTENZIONE: Il controllo del ritorno avviene SOLO qui
            if (!_agent.pathPending &&
                _agent.remainingDistance <= _agent.stoppingDistance)
            {
                Debug.Log($"EnemyBase: [{gameObject.name}] Nessuna traccia nel punto noto. Ritorno alla base!");
                SetState(EnemyState.Return);
            }
        }
    }

    // --- SEZIONE GIZMOS VISIVI ---
    private void OnDrawGizmosSelected()
    {
        if (_waypoints == null || _waypoints.Length == 0) return;

        // 1. Disegniamo il percorso della pattuglia
        Gizmos.color = Color.yellow;
        for (int i = 0; i < _waypoints.Length; i++)
        {
            if (_waypoints[i] != null)
            {
                // Disegna una sfera su ogni waypoint
                Gizmos.DrawWireSphere(_waypoints[i].position, 0.5f);

                // Disegna una linea verso il waypoint successivo (Pattern Circolare)
                int nextIndex = (i + 1) % _waypoints.Length;
                if (_waypoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(_waypoints[i].position, _waypoints[nextIndex].position);
                }
            }
        }

        // 2. Disegniamo la destinazione attuale dell'Agent (in verde)
        // Lo facciamo solo in PlayMode per evitare errori nell'Editor
        if (Application.isPlaying && _agent != null && _agent.hasPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _agent.destination);
        }
    }
}