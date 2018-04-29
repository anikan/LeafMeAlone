using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared;

namespace Server
{
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    public class NetworkServer
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        //Socket on server listening for requests.
        private Socket listener;

        //If false, then set the async call to accept requests.
        private bool isListening = false;

        //Socket to communicate with client.
        private Socket clientSocket;

        //List of packets for Game to process.
        public List<PlayerPacket> PlayerPackets = new List<PlayerPacket>();

        public NetworkServer()
        {
        }

        /// <summary>
        /// Open a socket for clients to conenct to.
        /// </summary>
        public void StartListening()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  

            IPAddress ipAddress = IPAddress.Loopback;//ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void CheckForConnections()
        {
            try
            {
                // Set the event to nonsignaled state.  
                //allDone.Reset();

                // Start an asynchronous socket to listen for connections.  
                if (!isListening)
                {
                    Console.WriteLine("Waiting for a connection...");

                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    isListening = true;
                }

                // Wait until a connection is made before continuing.  
                //allDone.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            clientSocket = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = clientSocket;
            clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                byte[] resizedBuffer = new byte[bytesRead];

                Buffer.BlockCopy(state.buffer, 0, resizedBuffer, 0, bytesRead);
                    
                PlayerPacket packet = PlayerPacket.Deserialize(resizedBuffer);

                PlayerPackets.Add(packet);
                
                // All the data has been read from the   
                // client. Display it on the console.  
                //Console.WriteLine("Read new player packet: Data : {0}",
                //    packet.ToString());

            }

            // Create a new state object for the next packet.  
            StateObject newState = new StateObject();
            newState.workSocket = handler;

            //Begin listening again for more packets.
            handler.BeginReceive(newState.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), newState);
        }

        /// <summary>
        /// Given a player, generate a PlayerPacket and send it.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="player"></param>
        public void SendPlayer(PlayerServer player)
        {
            PlayerPacket packet = ServerPacketFactory.CreatePacket(player);

            Console.WriteLine("Sending packet {0}.", packet.ToString());

            //PlayerPacket testPacket = PlayerPacket.Deserialize(PlayerPacket.Serialize(packet));

            Send(clientSocket, PlayerPacket.Serialize(packet));
        }

        private void Send(Socket handler, byte[] byteData)
        {
            if (handler != null)
            {
                // Begin sending the data to the remote device.  
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
            }

            else
            {
                Console.WriteLine("No socket connected.");
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.\n", bytesSent);

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
