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
            bool isPC = packet.ReadBool();
            string username = packet.ReadString();
            Server.clients[clientID].username = username;
            Server.clients[clientID].isPC = isPC;
            Console.WriteLine($"Player {username} with id {clientID} is now connected");

            //TODO: send lobby info to clients
            ServerSend.SendLobbyInfo(clientID);
        }

        public static void ReceiveUDPInit(int client, Packet packet)
        {
            string msg = packet.ReadString();
            Console.WriteLine($"Client {client}: {msg}");
        }

        public static void RequestLobbyInfo(int client, Packet packet)
        {
            int clientID = packet.ReadInt();
            ServerSend.SendExistingLobbyInfo(clientID);
        }

        public static void ReceiveStartGame(int client, Packet packet)
        {
            Server.StartGame();
            ServerSend.SendStartGame();
        }

        public static void ReceiveTransform(int client, Packet packet)
        {
            int clientID = packet.ReadInt();
            Vector3 position = packet.ReadVector();
            Quaternion rotation = packet.ReadQuaternion();
            ServerSend.UpdatePlayerTransform(clientID, position, rotation);
        }
    }
}
