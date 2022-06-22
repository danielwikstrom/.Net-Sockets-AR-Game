using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class PacketHandler : MonoBehaviour
{
    public static void ReadInitMsg(Packet packet)
    {
        string msg = packet.ReadString();
        int id = packet.ReadInt();

        Debug.Log("[SERVER]: " + msg );
        Client.instance.id = id;

        ClientSend.OnReceivedInitMsg();

        // UDP connection uses the same local port as tcp
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    /// <summary>
    /// This method takes an empty packed as it is only a request from the server to all clients to start the game
    /// </summary>
    /// <param name="packet"></param>
    public static void StartGame(Packet packet)
    {
        GameManager.instance.StartGame();
    }

    public static void ReadUDPInit(Packet packet)
    {
        string msg = packet.ReadString();
        Debug.Log("[SERVER]: " + msg);

        ClientSend.OnReceivedUDPInit();
        ClientSend.RequestLobbyInfo();
    }

    public static void ReadLobbyInfo(Packet packet)
    {
        string username = packet.ReadString();
        int id = packet.ReadInt();
        bool isPC = packet.ReadBool();
        Debug.Log("Player " + username + " is in session");
        if (isPC)
            GameManager.instance.InitPlayer(id, username, Vector3.zero, Quaternion.identity);
        else
            GameManager.instance.InitSpectator(id, username);
    }

    public static void UpdateRemotePlayerTransform(Packet packet)
    {
        int clientID = packet.ReadInt();
        Vector3 position = packet.ReadVector();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.instance.UpdatePlayerTransform(clientID, position, rotation);
    }

    public static void PlayerDisconnected(Packet packet)
    {
        int clientID = packet.ReadInt();
        GameManager.instance.PlayerDisconnected(clientID);
    }
}
