using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();

    public int id;
    public Rigidbody body;
    public int playerId;
    public Vector3 initialForce;
    public float explosionRadius = 1.5f;
    public float explosionDamage = 75f;

    private static int nextProjectileId = 1;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        projectiles.Add(id, this);
        ServerSend.SpawnProjectile(this, playerId);
        body.AddForce(initialForce);
        StartCoroutine(ExplodeAfterTime());
    }

    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    public void Initialise(Vector3 initialDirection, float forceStrength, int thrownByPlayer)
    {
        initialForce = initialDirection * forceStrength;
        playerId = thrownByPlayer;
    }

    private void Explode()
    {
        ServerSend.ProjectileExploded(this);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider collider in colliders)
        {
            if(collider.CompareTag("Player"))
            {
                collider.GetComponent<Player>().TakeDamage(explosionDamage);
            } else if (collider.CompareTag("Enemy"))
            {
                collider.GetComponent<Enemy>().TakeDamage(explosionDamage);
            }
        }
        projectiles.Remove(id);
        Destroy(gameObject);
    }

    private IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(10f);
        Explode();
    }
}
