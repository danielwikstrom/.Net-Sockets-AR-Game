using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class PacketHandler
    {
        public static void ReceiveInitMsg(int client, Packet packet)
        {
            int clientID = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"Player {username} with id {clientID} is now connected");

            //TODO: send lobby info to clients
        }

        public static void ReceiveUDPInit(int client, Packet packet)
        {
            string msg = packet.ReadString();
            Console.WriteLine($"Client {client}: {msg}");
        }
    }
}
