using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public enum PacketType
{
    TCPInitial = 1,
    UDPInitial
}
public class Client : MonoBehaviour
{

    public static Client instance;
    public static int dataBufferSize = 1024;

    //TODO assign on runtime to servers ip;
    public string ip = "127.0.0.1";
    public int port = 26950;
    public int id = 0;
    public TCP tcp;
    public UDP udp;

    public string username;
    //The device this client is playing on. True for PC, false for mobile.
    public bool isPC = true;

    private delegate void PacketManager(Packet packet);
    private static Dictionary<int, PacketManager> packetManagers;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
            Destroy(this);

        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    public void Connect()
    {
        InitializeClientData();
        tcp.Connect();
    }

    public void SetUsername(string username)
    {
        this.username = username;
    }

    public void SetDevice(bool isPC)
    {
        this.isPC = isPC;
    }

    public class TCP
    {
        public TcpClient socket;
        private readonly int id;
        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Packet receivedPacket;

        public void Connect()
        {
            Debug.Log("Attempting connection to " + instance.ip + " on port " + instance.port);
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectAsync, socket);
        }

        private void ConnectAsync(IAsyncResult result)
        {
            socket.EndConnect(result);
            if (!socket.Connected)
            {
                Debug.Log("Connection failed");
                return;
            }

            stream = socket.GetStream();
            receivedPacket = new Packet();
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveAsync, null);

        }

        public void SendPacket(Packet packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error sending data to server: " + e);
            }
        }

        private void ReceiveAsync(IAsyncResult result)
        {
            try
            {
                int receivedDataLength = stream.EndRead(result);
                if (receivedDataLength <= 0)
                {
                    return;
                    //probably disconnet client
                }
                byte[] data = new byte[receivedDataLength];
                Array.Copy(receiveBuffer, data, receivedDataLength);

                //Handle the received package
                receivedPacket.Reset(HandleDate(data));

                //After reading the data, we wait for the next packet to arrive
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveAsync, null);
            }
            catch (Exception e)
            {
                Debug.LogError("Error: " + e);
                //disconnect client
            }
        }

        private bool HandleDate(byte[] data)
        {
            int packetLength = 0;

            receivedPacket.SetBytes(data);

            //if more than 4 bytes(int size) means there is still data to be read, as the first bytes of the packet are the packet length
            if (receivedPacket.UnreadLength() >= sizeof(int))
            {
                packetLength = receivedPacket.ReadInt();
                if (packetLength <= 0)
                {
                    //packet is empty
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= receivedPacket.UnreadLength())
            {
                byte[] packetBytes = receivedPacket.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetID = packet.ReadInt();
                        packetManagers[packetID](packet);
                    }
                });

                packetLength = 0;

                //if more than 4 bytes(int size) means there is still data to be read, as the first bytes of the packet are the packet length
                if (receivedPacket.UnreadLength() >= sizeof(int))
                {
                    packetLength = receivedPacket.ReadInt();
                    if (packetLength <= 0)
                    {
                        //packet is empty
                        return true;
                    }
                }
            }
            if(packetLength <= 1)
                return true;

            //do not reset packet if there is still packet data that has not been read
            return false;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint ipEndPoint;

        public UDP()
        {
            ipEndPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        // takes the local port, not the server port
        public void Connect(int port)
        {
            socket = new UdpClient(port);
            socket.Connect(ipEndPoint);
            socket.BeginReceive(ReceiveAsync, null);

            using (Packet packet = new Packet())
            {
                SendPacket(packet);
            }
        }

        public void SendPacket(Packet packet)
        {
            try
            {
                packet.InsertInt(instance.id);
                if (socket != null)
                {
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error sending data: " + e);
            }
        }

        private void ReceiveAsync(IAsyncResult result)
        {
            try
            {
                byte[] data = socket.EndReceive(result, ref ipEndPoint);
                socket.BeginReceive(ReceiveAsync, null);

                if (data.Length < sizeof(int))
                {
                    return;
                }
                HandleData(data);
            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e);
            }
        }

        private void HandleData(byte[] data)
        {

            using (Packet packet = new Packet(data))
            {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(data))
                {
                    int packetID = packet.ReadInt();
                    packetManagers[packetID](packet);
                }
            });
        }
    }

    private void InitializeClientData()
    {
        packetManagers = new Dictionary<int, PacketManager>()
        {
            {(int)PacketType.TCPInitial, PacketHandler.ReadInitMsg},
            {(int)PacketType.UDPInitial, PacketHandler.ReadUDPInit}

        };
    }
}
