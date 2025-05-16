using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 25f;
    public float lifetime = 5f;
    public int damage = 10;
    public LayerMask hitLayer;

    private Rigidbody rb;
    private bool hasBeenFired = false;

    private GameObject shooter;

    private float idleTimer = 0f;
    private bool allowTimeout = false;

    public void EnableTimeout()
    {
        allowTimeout = true;
    }

    void Update()
    {
        if (allowTimeout && !hasBeenFired)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > 4f)
                Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Prevent motion until fired
    }

    public void SetShooter(GameObject whoShot)
    {
        shooter = whoShot;
    }

    // Called from animation event (Shoot)
    public void Fire(Vector3 direction)
    {
        rb.isKinematic = false;
        rb.velocity = direction.normalized * speed;
        hasBeenFired = true;
        Destroy(gameObject, lifetime);
    }
    void OnTriggerEnter(Collider other)
    {
        if (!hasBeenFired) return;

        // Ignore the shooter
        if (other.gameObject == shooter) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetHealth(-damage);
            }
        }

        Destroy(gameObject);
    }
}
