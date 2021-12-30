using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class ClientData
    {
        public static int dataBufferSize = 1024;
        public int id;
        public string username = "";
        public TCP tcp;
        public UDP udp;

        public ClientData(int id)
        {
            this.id = id;
            tcp = new TCP(this.id);
            udp = new UDP(this.id);
        }

        public class TCP
        {
            public TcpClient socket;
            private readonly int id;
            private NetworkStream stream;
            private byte[] receiveBuffer;
            private Packet receivedPacket;
            public TCP(int id)
            {
                this.id = id;
            }

            public void Connect(TcpClient socket)
            {
                this.socket = socket;
                this.socket.ReceiveBufferSize = dataBufferSize;
                this.socket.SendBufferSize = dataBufferSize;
                stream = this.socket.GetStream();
                receivedPacket = new Packet();
                receiveBuffer = new byte[dataBufferSize];
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveAsync, null);
                //send welocme packet

                ServerSend.SendInitMessage(this.id, "Succesfully conected to server");
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
                catch(Exception e)
                {
                    Console.WriteLine($"Error sending packet to client {id}: {e}");
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

                    //do stuff weith the received data
                    receivedPacket.Reset(HandleDate(data));


                    //After reading the data, we wait for the next packet to arrive
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveAsync, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e}");
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
                            Server.packetManagers[packetID](id, packet);
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
                if (packetLength <= 1)
                    return true;

                //do not reset packet if there is still packet data that has not been read
                return false;
            }

        }

        public class UDP
        {
            public IPEndPoint ipEndPoint;
            private int id;
            public UDP(int id)
            {
                this.id = id;
            }

            // takes the local port, not the server port
            public void Connect(IPEndPoint ipEndPoint)
            {
                this.ipEndPoint = ipEndPoint;

                ServerSend.SendInitUDP(id);
            }

            public void SendPacket(Packet packet)
            {
                Server.SendPacketUDP(ipEndPoint, packet);
            }

            

            public void HandleData(Packet packet)
            {
                int packetLength = packet.ReadInt();
                byte[] data = packet.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(data))
                    {
                        int packetID = packet.ReadInt();
                        Server.packetManagers[packetID](id, packet);
                    }
                });
                
            }
        }
    }
}
