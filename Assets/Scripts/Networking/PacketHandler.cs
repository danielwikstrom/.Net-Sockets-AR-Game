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
        Debug.Log("Player " + username + " is in session");
        GameManager.instance.InitPlayer(id, username, Vector3.zero, Quaternion.identity);
    }
}
