using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshSurface _navMeshSurface;

    [Header("Door Settings")]
    [SerializeField] private float _openPositionZ = 20f;
    [SerializeField] private float _closedPositionZ = 14f;
    [SerializeField] private float _moveSpeed = 3f;

    private bool _isOpen = false;
    private bool _isMoving = false; // Evita doppi input durante il movimento

    public void ToggleDoor()
    {
        if (_isMoving) return; // Ignora input se già in movimento
        
        _isOpen = !_isOpen;
        float targetZ = _isOpen ? _openPositionZ : _closedPositionZ;
        
        StartCoroutine(MoveDoor(targetZ));
    }

    private IEnumerator MoveDoor(float targetZ)
    {
        _isMoving = true;

        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(
            transform.position.x, 
            transform.position.y, 
            targetZ
        );

        // Muovi la porta gradualmente
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPos, 
                _moveSpeed * Time.deltaTime
            );
            yield return null; // Aspetta il prossimo frame
        }

        // Snap alla posizione esatta
        transform.position = targetPos;

        // ORA che la porta è ferma, ricalcola la NavMesh
        _navMeshSurface.BuildNavMesh();
        Debug.Log($"Porta {(_isOpen ? "APERTA" : "CHIUSA")} - NavMesh aggiornata");

        _isMoving = false;
    }
}