using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class GameController : MonoBehaviour
{
    // L'istanza globale accessibile da tutti
    public static GameController Instance { get; private set; }

    [Header("Respawn")]
    [SerializeField] private Transform _respawnPoint; 

    private NavMeshAgent _playerAgent;
    private PlayerController _playerController;

    private void Awake()
    {
        // Pattern Singleton: se esiste già un'istanza, distruggi questa
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Persiste tra le scene
        DontDestroyOnLoad(gameObject); 
    }

    private void Start()
    {
        // Cerca il player all'avvio
        GameObject player = GameObject.FindWithTag("Player");
        _playerAgent = player.GetComponent<NavMeshAgent>();
        _playerController = player.GetComponent<PlayerController>();
    }

    public void OnPlayerCaught()
    {
        Debug.Log("GameController: Player catturato! Respawn...");
        RespawnPlayer();
    }

    private void RespawnPlayer()
    {
        if (_respawnPoint == null)
        {
            Debug.LogError("GameController: _respawnPoint non assegnato!");
            return;
        }

        // Disabilita l'agent prima di teletrasportare
        // (Warp è il metodo corretto per spostare un NavMeshAgent)
        _playerAgent.Warp(_respawnPoint.position);

        Debug.Log($"GameController: Player rispawnato a {_respawnPoint.position}");
    }
}