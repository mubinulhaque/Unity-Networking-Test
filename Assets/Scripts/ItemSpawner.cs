﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static Dictionary<int, ItemSpawner> spawners = new Dictionary<int, ItemSpawner>();
    public int spawnerId;
    public bool hasItem = false;

    private static int nextSpawnerId = 1;

    private void Start()
    {
        hasItem = false;
        spawnerId = nextSpawnerId;
        nextSpawnerId++;
        spawners.Add(spawnerId, this);

        StartCoroutine(SpawnItem());
    }

    private void OnTriggerEnter(Collider other)
    {
        if(hasItem && other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            if(player.AttemptPickupItem())
            {
                ItemPickedUp(player.id);
            }
        }
    }

    private IEnumerator SpawnItem()
    {
        yield return new WaitForSeconds(10f);

        hasItem = true;
        ServerSend.ItemSpawned(spawnerId);
    }

    private void ItemPickedUp(int playerId)
    {
        hasItem = false;
        ServerSend.ItemPickedUp(spawnerId, playerId);
        StartCoroutine(SpawnItem());
    }
}
