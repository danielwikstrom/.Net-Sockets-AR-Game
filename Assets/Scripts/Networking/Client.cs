using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{

    public static Client instance;
    public static int dataBufferSize = 1024;

    //TODO assign on runtime to servers ip;
    public string ip = "127.0.0.1";
    public int port = 8888;
    public int id = 0;
    public TCP tcp;

    public string username;
    public bool isPC = true;


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
    }

    public void Connect()
    {
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
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveAsync, null);

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

                //do stuff weith the received data

                //After reading the data, we wait for the next packet to arrive
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveAsync, null);
            }
            catch (Exception e)
            {
                Debug.LogError("Error: " + e);
                //disconnect client
            }
        }
    }
}
