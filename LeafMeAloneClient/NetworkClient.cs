using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
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
        public static int BufferSize = 4096;
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

        private IPAddress address;

        public NetworkClient(IPAddress address)
        {
            this.address = address;
        }

        /// <summary>
        /// Try to connect to the host.
        /// </summary>
        public void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                //For testing purposes, connect to Loopback. 
                //IPAddress ipAddress = IPAddress.Loopback; // new IPAddress(IPAddress.Loopback);//ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(address, port);

                // Create a TCP/IP socket.  
                client = new Socket(address.AddressFamily,
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
         //   Console.WriteLine(client.Available);

            byte[] savedBuffer = new byte[0];

            while (client.Available > 0)
            {
                //Saving amount available as it may change while waiting. 
                int amountAvailable = client.Available;

                //Initialize buffer size to be num bytes available or the full buffer size.
                byte[] buffer = new byte[Math.Min(amountAvailable + savedBuffer.Length, StateObject.BufferSize)];

                Buffer.BlockCopy(savedBuffer, 0, buffer, 0, savedBuffer.Length);

                //Set how many bytes to read this iteration.
                int bytesToReceive = Math.Min(amountAvailable, StateObject.BufferSize - savedBuffer.Length);

                int bytesToRead =
                    client.Receive(buffer, savedBuffer.Length, bytesToReceive, 0);

                bytesToRead += savedBuffer.Length;

                savedBuffer = new byte[0];

                while (buffer.Length > 0)
                {
                    Packet objectPacket =
                        Packet.Deserialize(buffer, out int bytesRead);

                    //If we need more bytes to continue, break.
                    if (objectPacket == null)
                    {
                        savedBuffer = buffer;
                        break;
                    }

                    PacketQueue.Add(objectPacket);

                    buffer = buffer.Skip(bytesRead).ToArray();
                    bytesToRead -= bytesRead;
                }
            }
        }

        /// <summary>
        /// Send the given data to the server.
        /// </summary>
        /// <param name="data">Byte array of data</param>
        public void Send(byte[] data)
        {
            //Console.WriteLine(BitConverter.ToString(data));
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
