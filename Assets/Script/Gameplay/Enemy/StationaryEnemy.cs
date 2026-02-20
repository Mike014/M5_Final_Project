using UnityEngine;

public class StationaryEnemy : EnemyBase
{
    [Header("Rotation")]
    [SerializeField] private float _rotationInterval = 2f;
    [SerializeField] private float _rotationAngle = 90f;

    private float _rotationTimer;
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    protected override void Awake()
    {
        base.Awake();
        _startPosition = transform.position;
        _startRotation = transform.rotation;

        // LOG DI INIZIALIZZAZIONE: Utile per confermare che l'Awake è scattato
        Debug.Log($"StationaryEnemy: [{gameObject.name}] Awake. StartPosition salvata a {_startPosition}");
    }

    protected override void HandleIdle()
    {
        _rotationTimer += Time.deltaTime;

        if (_rotationTimer >= _rotationInterval)
        {
            _rotationTimer = 0f;
            transform.Rotate(0f, _rotationAngle, 0f);

            // LOG DI AZIONE: Lo mettiamo DENTRO l'if. 
            // Verrà chiamato solo ogni X secondi, non ad ogni frame!
            Debug.Log($"StationaryEnemy: [{gameObject.name}] Ruotato di {_rotationAngle}°. Nuova direzione Y: {transform.eulerAngles.y}");
        }
    }

    protected override void HandleReturn()
    {
        // LOG SPIA: Questo ci dirà se il Regista ci ha detto di leggere questo capitolo!
        // Usiamo Time.frameCount per non impazzire se il log spamma
        if (Time.frameCount % 60 == 0) // Logga solo 1 volta al secondo (circa)
        {
            Debug.Log($"StationaryEnemy: Sono nello stato RETURN! Distanza rimanente: {_agent.remainingDistance}");
        }

        _agent.SetDestination(_startPosition);

        if (!_agent.pathPending &&
            _agent.remainingDistance <= _agent.stoppingDistance)
        {
            Debug.Log($"StationaryEnemy: [{gameObject.name}] Arrivato alla base. Ripristino rotazione e torno a Idle.");
            transform.rotation = _startRotation;
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

    // Blocco temporanemante NON USATO
    // Usiamo OnDrawGizmosSelected invece di OnDrawGizmos
    // private void OnDrawGizmosSelected()
    // {
    //     // 1. Disegna la direzione in cui sta guardando il nemico (Rosso)
    //     Gizmos.color = Color.red;
    //     // Partiamo dal nemico e tracciamo una linea in avanti (transform.forward) lunga 3 metri
    //     Gizmos.DrawRay(transform.position, transform.forward * 3f);

    //     // 2. Disegna la posizione iniziale e la linea di ritorno (Blu)
    //     // Application.isPlaying serve perché _startPosition è calcolata in Awake (durante il Play).
    //     // Se non lo mettessimo, nell'Editor disegnerebbe una linea verso le coordinate 0,0,0.
    //     if (Application.isPlaying)
    //     {
    //         Gizmos.color = Color.cyan;
    //         // Disegna una sfera a fil di ferro nel punto di origine
    //         Gizmos.DrawWireSphere(_startPosition, 0.5f);
    //         // Disegna un "filo" che collega il nemico alla sua base
    //         Gizmos.DrawLine(transform.position, _startPosition);
    //     }
    // }
}