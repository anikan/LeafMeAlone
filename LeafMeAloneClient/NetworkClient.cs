using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared;

namespace Client
{


    /// <summary>
    /// State object for receiving data from remote device. 
    /// </summary>
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public static int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    /// <summary>
    /// Handles network activity for sending and receiving packets.
    /// </summary>
    public class NetworkClient
    {
        // The port number for the remote device.  
        private const int port = 11000;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.  
        public String response = String.Empty;

        //Socket to communicate with the server.
        private Socket client;

        //List of received packets. Populated by ReadCallback
        public List<Packet> PacketQueue = new List<Packet>();

        /// <summary>
        /// Try to connect to the host.
        /// </summary>
        public void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                //For testing purposes, connect to Loopback. 
                IPAddress ipAddress = IPAddress.Loopback; // new IPAddress(IPAddress.Loopback);//ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Finalize the connection
        /// </summary>
        /// <param name="ar">Stores buffer and socket.</param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Receive packets from the server and add them to the appropriate queue.
        /// </summary>
        public void Receive()
        {
            byte[] buffer = new byte[StateObject.BufferSize];
            if (client.Available > 0)
            {
                int bytesRead =
                    client.Receive(buffer, 0, StateObject.BufferSize, 0);

                // Read everything but the first byte, which gives the packet
                // Type
                byte[] resizedBuffer = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, resizedBuffer, 0, bytesRead);

                ProcessPacket((PacketType) buffer[0], resizedBuffer);
            }
        }

        private void ProcessPacket(PacketType packetType, byte[] resizedBuffer)
        {
            switch (packetType)
            {
                case PacketType.CreateObjectPacket:
                    PacketQueue.Add(
                        CreateObjectPacket.Deserialize(resizedBuffer)
                        );
                    break;
                case PacketType.PlayerPacket:
                    PacketQueue.Add(
                        PlayerPacket.Deserialize(resizedBuffer)
                        );
                    break;
                case PacketType.LeafPacket:
                    break;
            }
        }

        /// <summary>
        /// Send the given data to the server.
        /// </summary>
        /// <param name="data">Byte array of data</param>
        public void Send(byte[] data)
        {
            Console.WriteLine(BitConverter.ToString(data));
            // Begin sending the data to the remote device.  
            client.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        /// <summary>
        /// Called when send succeeds or fails.
        /// </summary>
        /// <param name="ar">Stores buffer and socket.</param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            sendDone.WaitOne();

        }
    }
}
