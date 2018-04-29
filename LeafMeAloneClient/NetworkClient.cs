using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared;

namespace LeafMeAloneClient
{


    /// <summary>
    /// State object for receiving data from remote device. 
    /// </summary>
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
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
        public List<PlayerPacket> PlayerPackets = new List<PlayerPacket>();

        /// <summary>
        /// Try to connect to the host.
        /// </summary>
        public void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                //IPHostEntry ipHostInfo = Dns.GetHostEntry("host.contoso.com");

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
        /// Start callback to receive packets from the server.
        /// </summary>
        public void Receive()
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// The callback to receive packets. Currently only accepts player packets.
        /// </summary>
        /// <param name="ar">Stores buffer and socket.</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                //Console.WriteLine("Received data");

                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    //Store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    byte[] resizedBuffer = new byte[bytesRead];
                    Buffer.BlockCopy(state.buffer, 0, resizedBuffer, 0, bytesRead);
                    
                    PlayerPacket packet = PlayerPacket.Deserialize(resizedBuffer);

                    Console.WriteLine("Received packet {0}.", packet.ToString());

                    PlayerPackets.Add(packet);
                }

                // Get the next packet
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            receiveDone.WaitOne();

        }

        /// <summary>
        /// Send the given data to the server.
        /// </summary>
        /// <param name="data">Byte array of data</param>
        public void Send(byte[] data)
        {
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
