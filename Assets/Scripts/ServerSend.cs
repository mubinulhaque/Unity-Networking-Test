using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend : MonoBehaviour
{
    #region Sending Packets to Individuals
    private static void SendTCPData(int clientId, Packet packet)
    {
        packet.WriteLength();
        Server.clients[clientId].tcp.SendData(packet);
    }

    private static void SendUDPData(int clientId, Packet packet)
    {
        packet.WriteLength();
        Server.clients[clientId].udp.SendData(packet);
    }
    #endregion

    #region Sending TCP Data to Multiple Players
    private static void SendTCPDataToAll(Packet packet)
    {
        packet.WriteLength();

        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendTCPDataToAll(int exceptionClient, Packet packet)
    {
        packet.WriteLength();

        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            if (i != exceptionClient)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
    }
    #endregion

    #region Sending UDP Data to Multiple Players
    private static void SendUDPDataToAll(Packet packet)
    {
        packet.WriteLength();

        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUDPDataToAll(int exceptionClient, Packet packet)
    {
        packet.WriteLength();

        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            if (i != exceptionClient)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }
    #endregion

    #region Packets
    public static void Welcome(int toClient, string message)
    {
        using (Packet packet = new Packet((int)ServerPackets.welcome))
        {
            packet.Write(message);
            packet.Write(toClient);

            SendTCPData(toClient, packet);
        }
    }

    public static void SpawnPlayer(int clientId, Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            packet.Write(player.id);
            packet.Write(player.username);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            SendTCPData(clientId, packet);
        }
    }

    public static void PlayerPosition(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerPosition))
        {
            packet.Write(player.id);
            packet.Write(player.transform.position);

            SendUDPDataToAll(packet);
        }
    }

    public static void PlayerRotation(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerRotation))
        {
            packet.Write(player.id);
            packet.Write(player.transform.rotation);

            SendUDPDataToAll(player.id, packet);
        }
    }

    public static void PlayerDisconnected(int clientId)
    {
        using(Packet packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            packet.Write(clientId);
            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerHealth(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerHealth))
        {
            packet.Write(player.id);
            packet.Write(player.health);
            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerRespawned(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerRespawned))
        {
            packet.Write(player.id);
            SendTCPDataToAll(packet);
        }
    }

    public static void CreateItemSpawner(int clientId, int spawnerId, Vector3 position, bool hasItem)
    {
        using(Packet packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            packet.Write(spawnerId);
            packet.Write(position);
            packet.Write(hasItem);
            SendTCPData(clientId, packet);
        }
    }

    public static void ItemSpawned(int spawnerId)
    {
        using(Packet packet = new Packet((int)ServerPackets.itemSpawned))
        {
            packet.Write(spawnerId);
            SendTCPDataToAll(packet);
        }
    }

    public static void ItemPickedUp(int spawnerId, int playerId)
    {
        using (Packet packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            packet.Write(spawnerId);
            packet.Write(playerId);
            SendTCPDataToAll(packet);
        }
    }

    public static void SpawnProjectile(Projectile projectile, int playerId)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnProjectile))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);
            packet.Write(playerId);
            SendTCPDataToAll(packet);
        }
    }

    public static void ProjectilePosition(Projectile projectile)
    {
        using (Packet packet = new Packet((int)ServerPackets.projectilePosition))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);
            SendUDPDataToAll(packet);
        }
    }

    public static void ProjectileExploded(Projectile projectile)
    {
        using (Packet packet = new Packet((int)ServerPackets.projectileExploded))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);
            SendTCPDataToAll(packet);
        }
    }

    public static void SpawnEnemy(Enemy enemy)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnEnemy))
        {
            SendTCPDataToAll(SpawnEnemyData(enemy, packet));
        }
    }

    public static void SpawnEnemy(int clientId, Enemy enemy)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnEnemy))
        {
            SendTCPData(clientId, SpawnEnemyData(enemy, packet));
        }
    }

    private static Packet SpawnEnemyData(Enemy enemy, Packet packet)
    {
        packet.Write(enemy.id);
        packet.Write(enemy.transform.position);
        return packet;
    }

    public static void EnemyPosition(Enemy enemy)
    {
        using (Packet packet = new Packet((int)ServerPackets.enemyPosition))
        {
            packet.Write(enemy.id);
            packet.Write(enemy.transform.position);

            SendTCPDataToAll(packet);
        }
    }

    public static void EnemyHealth(Enemy enemy)
    {
        using (Packet packet = new Packet((int)ServerPackets.enemyHealth))
        {
            packet.Write(enemy.id);
            packet.Write(enemy.health);

            SendTCPDataToAll(packet);
        }
    }
    #endregion
}
