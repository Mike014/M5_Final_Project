using UnityEngine;
using TMPro;

public class ButtonInteraction : MonoBehaviour
{
    [SerializeField] private DoorController _targetDoor;
    [SerializeField] private float _interactionRange = 2f;
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;
    [SerializeField] private GameObject _interactionCanvas; // Trascina il Canvas figlio

    private Transform _player;
    private bool _playerInRange = false;

    void Awake()
    {
        _player = GameObject.FindWithTag("Player").transform;

        if (_targetDoor == null)
            Debug.LogError($"ButtonInteraction: [{gameObject.name}] _targetDoor NON assegnata!");
        
        // Assicura che il canvas parta disattivato
        if (_interactionCanvas != null)
            _interactionCanvas.SetActive(false);
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, _player.position);
        bool wasInRange = _playerInRange;
        _playerInRange = distance <= _interactionRange;

        // Mostra/nascondi canvas solo quando cambia stato
        if (_playerInRange != wasInRange)
        {
            _interactionCanvas.SetActive(_playerInRange);
            Debug.Log($"ButtonInteraction: Canvas {(_playerInRange ? "VISIBILE" : "NASCOSTO")}");
        }

        if (_playerInRange && Input.GetKeyDown(_interactionKey))
        {
            Debug.Log($"ButtonInteraction: Tasto {_interactionKey} premuto → chiamo ToggleDoor()");
            _targetDoor.ToggleDoor();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _interactionRange);
    }
}