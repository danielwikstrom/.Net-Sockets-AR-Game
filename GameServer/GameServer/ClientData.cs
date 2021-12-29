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
        public TCP tcp;

        public ClientData(int id)
        {
            this.id = id;
            tcp = new TCP(this.id);
        }

        public class TCP
        {
            public TcpClient socket;
            private readonly int id;
            private NetworkStream stream;
            private byte[] receiveBuffer;
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
                receiveBuffer = new byte[dataBufferSize];
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveAsync, null);
                //send welocme packet
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
                    Console.WriteLine($"Error: {e}");
                    //disconnect client
                }
            }

        }
    }
}
