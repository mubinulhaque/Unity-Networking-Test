using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class ServerHandle
{
    public static void WelcomeReceived(int clientId, Packet packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log($"{Server.clients[clientId].tcp.socket.Client.RemoteEndPoint} connected and is now player {clientId}");

        if (clientId != clientIdCheck)
        {
            Debug.Log($"Player \"{username}\" (ID: {clientId} has assumed the wrong client ID ({clientIdCheck})!");
        }

        Server.clients[clientId].SendIntoGame(username);
    }

    public static void PlayerMovement(int clientId, Packet packet)
    {
        bool[] inputs = new bool[packet.ReadInt()];

        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = packet.ReadBool();
        }

        Quaternion rotation = packet.ReadQuaternion();

        Server.clients[clientId].player.SetInput(inputs, rotation);
    }

    public static void PlayerShoot(int clientId, Packet packet)
    {
        Vector3 shootDirection = packet.ReadVector3();
        Server.clients[clientId].player.Shoot(shootDirection);
    }

    public static void PlayerThrow(int clientId, Packet packet)
    {
        Vector3 throwDirection = packet.ReadVector3();
        Server.clients[clientId].player.ThrowItem(throwDirection);
    }
}
