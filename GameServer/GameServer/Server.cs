using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port {get; private set;}

        private static TcpListener tcpListener;
        public static Dictionary<int, ClientData> clients = new Dictionary<int, ClientData>();

        public static void Start(int maxPlayers, int port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

            Console.WriteLine("Starting Server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectAsync), null);

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
                    Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
                    return;
                }
            }

            //If the loop finishes the server is full so the new incoming client cannot connect
            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect as server is full");
        }

        private static void InitializeServerData()
        {
            //we initializze a dictionary with all possible clients, which will be assigned as they connect to the server
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new ClientData(i));
            }
        }
    }
}
