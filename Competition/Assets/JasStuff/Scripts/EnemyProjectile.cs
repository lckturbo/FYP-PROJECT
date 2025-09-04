using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private int _atkDmg;
    [SerializeField] private Rigidbody2D _rb2d;
    [SerializeField] private float speed;

    private void OnDisable()
    {
        CancelInvoke(nameof(Deactivate));
    }
    public void Init(Vector2 dir, int dmg)
    {
        if (_rb2d == null)
            _rb2d = GetComponent<Rigidbody2D>();
        _atkDmg = dmg;
        _rb2d.velocity = dir.normalized * speed;

        CancelInvoke(nameof(Deactivate));
        Invoke(nameof(Deactivate), 1.0f);
    }

    private void Deactivate()
    {
        _rb2d.velocity = Vector3.zero;
        ProjectilePool.instance.ReturnProjectile(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
                playerHealth.TakeDamage(_atkDmg);
            //Destroy(gameObject);

            ProjectilePool.instance.ReturnProjectile(gameObject);
        }

        //ProjectilePool.instance.ReturnProjectile(gameObject);
        //Destroy(gameObject);
    }   
}
