using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerSend
    {

        private static void SendPacketTCP(int client, Packet packet)
        {
            packet.WriteLength();
            Server.clients[client].tcp.SendPacket(packet);
        }

        private static void SendPacketUDP(int client, Packet packet)
        {
            packet.WriteLength();
            Server.clients[client].udp.SendPacket(packet);
        }

        private static void SendPacketToAllTCP(Packet packet, int excludedClient = -1)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (i != excludedClient)
                {
                    Server.clients[i].tcp.SendPacket(packet);
                }
            }
        }

        private static void SendPacketToAllUDP(Packet packet, int excludedClient = -1)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (i != excludedClient)
                {
                    Server.clients[i].udp.SendPacket(packet);
                }
            }
        }




        //first message sent to a client when it first connects to the server
        public static void SendInitMessage(int client, string msg)
        {
            // calling a packet with the "using" block will make sure to dispose the packet automatically, as the packets
            // use the interface "IDisposable".
            using (Packet packet = new Packet((int)PacketType.TCPInitial))
            {
                packet.Write(msg);
                packet.Write(client);

                SendPacketTCP(client, packet);
            }
        }

        public static void SendInitUDP(int client)
        {
            using (Packet packet = new Packet((int)PacketType.UDPInitial))
            {
                packet.Write("UDP Connection Succesful");

                SendPacketUDP(client, packet);
            }
        }
    }
}
