using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{

    public enum PacketType
    {
        TCPInitial = 1,
        UDPInitial,
        LobbyInfo,
        LobbyRequest
    }

    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        private static TcpListener tcpListener;
        private static UdpClient udpClient;
        public static Dictionary<int, ClientData> clients = new Dictionary<int, ClientData>();
        //This delegate function is used to determine with method in PacketHandler must be used for each packet received
        // e.g: The initial packet has to be decoded diferently than a packet sending the players location
        public delegate void PacketManager(int client, Packet packet);
        public static Dictionary<int, PacketManager> packetManagers;

        public static void Start(int maxPlayers, int port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

            Console.WriteLine("Starting Server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectAsync), null);

            udpClient = new UdpClient(Port);
            udpClient.BeginReceive(UDPReceiveAsync, null);

            Console.WriteLine($"Server started on port: {Port}");

        }

        private static void TCPConnectAsync(IAsyncResult result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);

            //After accepting a client, the server keeps waiting for another client to connect
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectAsync), null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint} ...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                //if clients socket is null it means that it has not been assigned to a connected client yet
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    Console.WriteLine($"Client connected via TCP: {client.Client.RemoteEndPoint}");
                    return;
                }
            }

            //If the loop finishes the server is full so the new incoming client cannot connect
            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect as server is full");
        }

        private static void UDPReceiveAsync(IAsyncResult result)
        {
            try
            {

                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.EndReceive(result, ref ipEndPoint);
                udpClient.BeginReceive(UDPReceiveAsync, null);

                if (data.Length < sizeof(int))
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    int clientID = packet.ReadInt();
                    if (clients[clientID].udp.ipEndPoint == null)
                    {
                        clients[clientID].udp.Connect(ipEndPoint);
                        Console.WriteLine($"Client connected via UDP: {clients[clientID].udp.ipEndPoint}");
                        //If its the first packet just to make the connection there is no need to handle the data
                        return;
                    }

                    if (clients[clientID].udp.ipEndPoint.ToString() == ipEndPoint.ToString())
                    { 
                        clients[clientID].udp.HandleData(packet);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error receiving udp data: {e}");
            }
        }

        public static void SendPacketUDP(IPEndPoint ipEndPoint, Packet packet)
        {
            try
            {
                if (ipEndPoint != null)
                {
                    udpClient.BeginSend(packet.ToArray(), packet.Length(), ipEndPoint, null, null);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending udp data: {e}");
            }
        }

        private static void InitializeServerData()
        {
            //we initializze a dictionary with all possible clients, which will be assigned as they connect to the server
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new ClientData(i));
            }

            packetManagers = new Dictionary<int, PacketManager>()
            {
                { (int)PacketType.TCPInitial, PacketHandler.ReceiveInitMsg},
                { (int)PacketType.UDPInitial, PacketHandler.ReceiveUDPInit},
                { (int)PacketType.LobbyRequest, PacketHandler.RequestLobbyInfo}
            };
        }
    }
}
