using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 3f;

    private float timer;
    private Vector2 direction;

    private void OnEnable()
    {
        timer = lifeTime;
    }

    public void Fire(Vector2 dir)
    {
        direction = dir.normalized;
        timer = lifeTime;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);

        timer -= Time.deltaTime;
        if (timer <= 0f)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log($"Hit enemy {collision.name}, dealt {damage}");
            // TODO: Apply damage
            gameObject.SetActive(false);
        }
    }
}
