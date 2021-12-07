using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System;

public class NetworkClient : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartClient();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartClient() 
    {
        byte[] bytes = new byte[1024];

        try
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                sender.Connect(remoteEP);
                Debug.Log("Scokect connected to " + sender.RemoteEndPoint.ToString());

                byte[] msg = Encoding.ASCII.GetBytes("This is a test <EOF>");

                int bytesSent = sender.Send(msg);

                int bytesRec = sender.Receive(bytes);
                Debug.Log("Echo test " + Encoding.ASCII.GetString(bytes, 0, bytesRec));

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (ArgumentNullException ane)
            {
                Debug.LogError("ArgumentNullException : " + ane.ToString());
            }
            catch (SocketException se)
            {
                Debug.LogError("SocketException : " + se.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError("Unexpected exception : " + e.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
}
