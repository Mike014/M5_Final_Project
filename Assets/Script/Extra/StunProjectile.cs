using UnityEngine;

public class StunProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 15f;
    [SerializeField] private float _lifetime = 3f; // Si autodistrugge dopo X secondi

    private void Start()
    {
        // Si distrugge da solo se non colpisce niente
        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        // Si muove in avanti nella direzione in cui è stato sparato
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignora il Player — il proiettile parte da lui
        if (other.CompareTag("Player")) return;

        Debug.Log($"StunProjectile: Colpito → {other.gameObject.name}");

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            Debug.Log($"StunProjectile: Stordisco {enemy.gameObject.name}");
            enemy.GetStunned();
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}