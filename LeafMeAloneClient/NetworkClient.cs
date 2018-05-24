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
using Shared.Packet;

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
        private const int port = 2302;
        private const int PACK_HEAD_SIZE = 5;

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

        public List<byte> ByteReceivedQueue = new List<byte>();
        //List of received packets. Populated by ReadCallback
        public List<BasePacket> PacketQueue = new List<BasePacket>();

        private IPAddress ipAddress;

        public NetworkClient(IPAddress ipAddress)
        {
            this.ipAddress = ipAddress;
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
                //IPEndPoint remoteEP = new IPEndPoint(address, port);
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
                //Console.WriteLine(e.ToString());
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

                StateObject state = new StateObject();
                state.workSocket = client;
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                               new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Fired when there is more data coming off the socet. Appends new bytes to the end of a dynamic queue.
        /// </summary>
        /// <param name="asyncResult">Stores the receive result.</param>
        public void ReceiveCallback(IAsyncResult asyncResult)
        {
            // Retrieve the state object and the client socket   
            // from the asynchronous state object.  
            StateObject state = (StateObject)asyncResult.AsyncState;
            Socket client = state.workSocket;

            // Read data from the remote device.  
            int bytesRead = client.EndReceive(asyncResult);

            // There might be more data, so store the data received so far.  
            lock (ByteReceivedQueue)
            {
                ByteReceivedQueue.AddRange(state.buffer.Take(bytesRead));
            }

            //  Start listening again
            client.BeginReceive(
                state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }

        /// <summary>
        /// Receives bytes from the ByteReceivedQueue and deserializes them into packets for later processing.
        /// </summary>
        public void Receive()
        {
            while (ByteReceivedQueue.Count > 0)
            {
                // If there is not enough data left to read the size of the next packet, do other game updates
                if (ByteReceivedQueue.Count < PacketUtil.PACK_HEAD_SIZE)
                {
                    break;
                }

                // Get packet size
                byte[] headerByteBuf = ByteReceivedQueue.GetRange(0, PacketUtil.PACK_HEAD_SIZE).ToArray();
                int packetSize = BitConverter.ToInt32(headerByteBuf, 1);

                // If there is not enough data left to read the next packet, do other game updates
                if (ByteReceivedQueue.Count < packetSize + PacketUtil.PACK_HEAD_SIZE)
                {
                    break;
                }

                // Get full packet and add it to the queue 
                byte[] packetData = ByteReceivedQueue.GetRange(PacketUtil.PACK_HEAD_SIZE, packetSize).ToArray();
                byte[] fullPacket = headerByteBuf.Concat(packetData).ToArray();
                BasePacket packet = PacketUtil.Deserialize(fullPacket);
                PacketQueue.Add(packet);

                // Remove the read data 
                lock (ByteReceivedQueue)
                {
                    ByteReceivedQueue.RemoveRange(0, packetSize + PacketUtil.PACK_HEAD_SIZE);
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
