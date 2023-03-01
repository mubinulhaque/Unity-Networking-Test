using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Variables
    public static int maxEnemies = 5;
    public static Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();

    public int id;
    public EnemyState state;
    public Player target;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 8f;
    public float health;
    public float maxHealth = 100f;
    public float detectionRange = 30f;
    public float shootRange = 15f;
    public float shootAccuracy = .1f;
    public float idleDuration = 1f;
    public float patrolDuration = 3f;

    private static int nextEnemyId = 1;

    private bool isPatrolRoutineRunning;
    private float yVelocity = 0;
    #endregion

    #region Unity Methods
    private void Start()
    {
        id = nextEnemyId;
        nextEnemyId++;
        //health = maxHealth;
        enemies.Add(id, this);
        ServerSend.SpawnEnemy(this);
        state = EnemyState.patrol;
        gravity *= Time.deltaTime * Time.deltaTime;
        patrolSpeed *= Time.deltaTime;
        chaseSpeed *= Time.deltaTime;
    }

    private void Update()
    {
        if(transform.position.y < -10)
        {
            TakeDamage(maxHealth);
        }
    }

    private void FixedUpdate()
    {
        switch(state)
        {
            case EnemyState.idle:
                LookForPlayer();
                break;

            case EnemyState.patrol:
                if(!LookForPlayer())
                {
                    Patrol();
                }
                break;

            case EnemyState.chase:
                Chase();
                break;

            case EnemyState.attack:
                Attack();
                break;

            default:
                break;
        }
    }
    #endregion

#region Private Methods
    private bool LookForPlayer()
    {
        foreach(Client client in Server.clients.Values)
        {
            if(client.player != null)
            {
                Vector3 enemyToPlayer = client.player.transform.position - transform.position;

                if(enemyToPlayer.magnitude <= detectionRange)
                {
                    if(Physics.Raycast(shootOrigin.position, enemyToPlayer, out RaycastHit hit, detectionRange))
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            target = hit.collider.GetComponent<Player>();
                            if (isPatrolRoutineRunning)
                            {
                                isPatrolRoutineRunning = false;
                                StopCoroutine(StartPatrol());
                            }

                            state = EnemyState.chase;
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private void Patrol()
    {
        if(!isPatrolRoutineRunning)
        {
            StartCoroutine(StartPatrol());
        }

        Move(transform.forward, patrolSpeed);
    }

    private IEnumerator StartPatrol() {
        isPatrolRoutineRunning = true;
        Vector2 randomPatrolDirection = Random.insideUnitCircle.normalized;
        transform.forward = new Vector3(randomPatrolDirection.x, 0f, randomPatrolDirection.y);

        yield return new WaitForSeconds(patrolDuration);

        state = EnemyState.idle;

        yield return new WaitForSeconds(idleDuration);

        state = EnemyState.patrol;
        isPatrolRoutineRunning = false;
    }

    private void Chase()
    {
        if(CanSeeTarget())
        {
            Vector3 enemyToPlayer = target.transform.position - transform.position;

            if(enemyToPlayer.magnitude <= shootRange)
            {
                state = EnemyState.attack;
            } else
            {
                Move(enemyToPlayer, chaseSpeed);
            }
        } else
        {
            target = null;
            state = EnemyState.patrol;
        }
    }

    private void Attack()
    {
        if (CanSeeTarget())
        {
            Vector3 enemyToPlayer = target.transform.position - transform.position;
            transform.forward = new Vector3(enemyToPlayer.x, 0f, enemyToPlayer.z);

            if (enemyToPlayer.magnitude <= shootRange)
            {
                Shoot(enemyToPlayer);
            }
            else
            {
                Move(enemyToPlayer, chaseSpeed);
            }
        }
        else
        {
            target = null;
            state = EnemyState.patrol;
        }
    }

    private void Move(Vector3 direction, float speed)
    {
        direction.y = 0f;
        transform.forward = direction;
        Vector3 movement = transform.forward * speed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
        }
        yVelocity += gravity;
        movement.y = yVelocity;
        controller.Move(movement);
        ServerSend.EnemyPosition(this);
    }

    private void Shoot(Vector3 shootDirection)
    {
        if (Physics.Raycast(shootOrigin.position, shootDirection, out RaycastHit hit, shootRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if(Random.value <= shootAccuracy)
                {
                    hit.collider.GetComponent<Player>().TakeDamage(50f);
                }
            }
        }
    }
    private bool CanSeeTarget()
    {
        if (target == null)
        {
            return false;
        }

        if (Physics.Raycast(shootOrigin.position, target.transform.position - transform.position, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            health = 0f;
            enemies.Remove(id);
            Destroy(gameObject);
        }

        ServerSend.EnemyHealth(this);
    }

}

public enum EnemyState
{
    idle,
    patrol,
    chase,
    attack
}
