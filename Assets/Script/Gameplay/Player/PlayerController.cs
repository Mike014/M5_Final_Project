using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    // Referenza all'agent
    private NavMeshAgent _agent;

    // La camera da cui partiamo per il Raycast
    // (deve puntare alla camera principale)
    private Camera _camera;

    // Riferimento al "Ground"
    [SerializeField] private LayerMask _groundLayer;

    [Header("Combat")]
    [SerializeField] private GameObject _stunProjectilePrefab; // Prefab del proiettile
    [SerializeField] private Transform _firePoint;             // Empty figlio del Player
    [SerializeField] private KeyCode _fireKey = KeyCode.Space;

    // Utilizzo Awake piuttosto che Start in modo tale che i riferimenti siano stati già trovati
    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _camera = Camera.main;
    }

    void Update()
    {
        // Input.GetMouseButtonDown(0) = click sinistro
        // "Down" = solo il frame in cui viene premuto (non held)
        if (Input.GetMouseButtonDown(0))
        {
            MoveToClickPosition();
        }

        if (Input.GetKeyDown(_fireKey))
            FireStunProjectile();
    }

    private void MoveToClickPosition()
    {
        // ScreenPointToRay converte la posizione 2D del mouse
        // in un raggio 3D che parte dalla camera
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        // RaycastHit contiene tutte le info sul punto colpito
        RaycastHit hit;

        // Physics.Raycast lancia il raggio nella scena
        // "out hit" popola la struct con i dati del collider colpito
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _groundLayer))
        {
            // SetDestination è il cuore: passiamo il punto 3D
            // all'agent che calcola il percorso automaticamente
            // Debug.Log($"Ho appena cliccato su: {hit.collider.name}");
            _agent.SetDestination(hit.point);
        }
    }

    private void FireStunProjectile()
    {
        if (_stunProjectilePrefab == null || _firePoint == null) return;

        // Calcola la direzione verso il mouse in world space
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundLayer))
        {
            Vector3 direction = (hit.point - _firePoint.position).normalized;
            direction.y = 0f; // Mantieni il proiettile orizzontale

            // Ruota il firePoint verso il target
            Quaternion rotation = Quaternion.LookRotation(direction);

            Instantiate(_stunProjectilePrefab, _firePoint.position, rotation);
            Debug.Log("Player: Proiettile stordente sparato!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Log QUALSIASI cosa tocchi il player, non solo i nemici
        Debug.Log($"PlayerController: OnTriggerEnter con → {other.gameObject.name} | Tag: {other.tag} | Layer: {other.gameObject.layer}");

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("PlayerController: È un nemico! Chiamo OnPlayerCaught...");
            GameController.Instance.OnPlayerCaught();
        }
    }
}
