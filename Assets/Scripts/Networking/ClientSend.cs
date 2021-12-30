using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendPacketTCP(Packet packet)
    {
        packet.WriteLength();
        Client.instance.tcp.SendPacket(packet);
    }

    private static void SendPacketUDP(Packet packet)
    {
        packet.WriteLength();
        Client.instance.udp.SendPacket(packet);

    }

    public static void OnReceivedInitMsg()
    {
        using (Packet packet = new Packet((int)PacketType.TCPInitial))
        {
            packet.Write(Client.instance.id);
            packet.Write(Client.instance.username);

            SendPacketTCP(packet);
        }
    }

    public static void OnReceivedUDPInit()
    {
        using (Packet packet = new Packet((int)PacketType.UDPInitial))
        {
            packet.Write("UDP connection succesful");

            SendPacketUDP(packet);
        }
    }

    public static void RequestLobbyInfo()
    {
        using (Packet packet = new Packet((int)PacketType.LobbyRequest))
        {
            packet.Write(Client.instance.id);

            SendPacketTCP(packet);
        }
    }

    public static void StartGame()
    {
        using (Packet packet = new Packet((int)PacketType.StartGame))
        {
            SendPacketTCP(packet);
        }
    }

    public static void SendTransform(Vector3 position, Quaternion rotation)
    {
        using (Packet packet = new Packet((int)PacketType.UpdateTransform))
        {
            packet.Write(Client.instance.id);
            packet.Write(position);
            packet.Write(rotation);

            SendPacketUDP(packet);
        }
    }
}
