using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int clientID, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    public static void Start(int maxPlayers, int port)
    {
        MaxPlayers = maxPlayers;
        Port = port;

        Debug.Log("Establishing TCP Connection...");

        InitialiseServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        udpListener = new UdpClient(port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Server started successfully on port {Port}!");
    }

    private static void TCPConnectCallback(IAsyncResult result)
    {
        TcpClient client = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(client);
                return;
            }
        }

        Debug.Log($"{client.Client.RemoteEndPoint} failed to connect. Reason: Server is full!");
    }

    private static void UDPReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpListener.EndReceive(result, ref clientEndpoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (data.Length < 4)
            {
                return;
            }

            using (Packet packet = new Packet(data))
            {
                int clientId = packet.ReadInt();

                if (clientId == 0)
                {
                    return;
                }

                if (clients[clientId].udp.endpoint == null)
                {
                    clients[clientId].udp.Connect(clientEndpoint);
                    return;
                }

                if (clients[clientId].udp.endpoint.ToString() == clientEndpoint.ToString())
                {
                    clients[clientId].udp.HandleData(packet);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error receiving UDP data: {e}");
        }
    }

    public static void SendUDPData(IPEndPoint clientEndpoint, Packet packet)
    {
        try
        {
            if (clientEndpoint != null)
            {
                udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndpoint, null, null);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error sending data to {clientEndpoint} via UDP: {e}");
        }
    }

    private static void InitialiseServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int) ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived},
                {(int) ClientPackets.playerMovement, ServerHandle.PlayerMovement},
                {(int) ClientPackets.playerShoot, ServerHandle.PlayerShoot},
                {(int) ClientPackets.playerThrow, ServerHandle.PlayerThrow}
            };

        Debug.Log("Initialised packets.");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
