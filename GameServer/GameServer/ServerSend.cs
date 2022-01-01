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

        /// <summary>
        /// Send relevant player information to all connected players
        /// </summary>
        /// <param name="client"></param>
        public static void SendLobbyInfo(int client)
        {
            // calling a packet with the "using" block will make sure to dispose the packet automatically, as the packets
            // use the interface "IDisposable".

            using (Packet packet = new Packet((int)PacketType.LobbyInfo))
            {
                packet.Write(Server.clients[client].username);
                packet.Write(client);
                packet.Write(Server.clients[client].isPC);
                Console.WriteLine($"Sending player {Server.clients[client].username} lobby info to all clients...");
                SendPacketToAllTCP(packet);
            }
        }

        public static void SendExistingLobbyInfo(int client)
        {
            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (Server.clients[i].username != "" && i != client)
                {
                    using (Packet packet = new Packet((int)PacketType.LobbyInfo))
                    {
                        packet.Write(Server.clients[i].username);
                        packet.Write(i);
                        packet.Write(Server.clients[i].isPC);
                        Console.WriteLine($"Sending player {Server.clients[client].username} existing lobby info...");
                        SendPacketTCP(client, packet);
                    }
                }
            }
        }

        public static void SendStartGame()
        {
            using (Packet packet = new Packet((int)PacketType.StartGame))
            {
                SendPacketToAllTCP(packet);
            }
        }

        /// <summary>
        /// Updates via UDP all clients with a specifics client new position and rotation
        /// </summary>
        /// <param name="client"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public static void UpdatePlayerTransform(int client, Vector3 position, Quaternion rotation)
        {
            using (Packet packet = new Packet((int)PacketType.UpdateTransform))
            {
                packet.Write(client);
                packet.Write(position);
                packet.Write(rotation);

                SendPacketToAllUDP(packet, client);
            }
        }

        public static void ClientDisconnected(int clientID)
        {
            using (Packet packet = new Packet((int)PacketType.PlayerDisconnected))
            {
                packet.Write(clientID);

                SendPacketToAllTCP(packet);
            }
        }
    }
}
