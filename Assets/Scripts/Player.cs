using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public int itemAmount = 0;
    public int maxItemAmount = 3;
    public string username;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float health;
    public float maxHealth = 100f;
    public float throwForce = 600f;

    private bool[] inputs;
    private float yVelocity = 0;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialise(int playerId, string playerName)
    {
        id = playerId;
        username = playerName;
        health = maxHealth;

        inputs = new bool[5];
    }

    private void Update()
    {
        if(transform.position.y < -10)
        {
            TakeDamage(maxHealth);
        }
    }

    public void FixedUpdate()
    {
        if(health <= 0f)
        {
            return;
        }

        Vector2 inputDirection = Vector2.zero;

        if (inputs[0])
        {
            inputDirection.y += 1;
        }
        else if (inputs[1])
        {
            inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            inputDirection.x -= 1;
        }
        else if (inputs[3])
        {
            inputDirection.x += 1;
        }

        Move(inputDirection);
    }

    private void Move(Vector2 inputDirection)
    {
        Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
        moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if(inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        moveDirection.y = yVelocity;
        controller.Move(moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInput(bool[] newInputs, Quaternion newRotation)
    {
        inputs = newInputs;
        transform.rotation = newRotation;
    }

    public void Shoot(Vector3 viewDirection)
    {
        if(health <= 0)
        {
            return;
        }

        if(Physics.Raycast(shootOrigin.position, viewDirection, out RaycastHit hit, 25f))
        {
            if(hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<Player>().TakeDamage(50f);
            } else if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.GetComponent<Enemy>().TakeDamage(50f);
            }
        }
    }

    public void ThrowItem(Vector3 viewDirection)
    {
        if(health <= 0f)
        {
            return;
        }

        if(itemAmount > 0)
        {
            itemAmount--;
            NetworkManager.instance.InstantiateProjectile(shootOrigin).Initialise(viewDirection, throwForce, id);
        }
    }

    public void TakeDamage(float damage)
    {
        if(health <= 0)
        {
            return;
        }

        health -= damage;

        if(health <= 0f)
        {
            health = 0f;
            controller.enabled = true;
            transform.position = new Vector3(0, 25f, 0);
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }

    public bool AttemptPickupItem()
    {
        if(itemAmount >= maxItemAmount)
        {
            return false;
        }

        itemAmount++;
        return true;
    }
}
