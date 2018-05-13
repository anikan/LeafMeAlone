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
        private List<Socket> clientSockets;

        //List of packets for Game to process.
        public List<PlayerPacket> PlayerPackets = new List<PlayerPacket>();

        public IPAddress address;

        public NetworkServer(IPAddress address)
        {
            clientSockets = new List<Socket>();
            this.address = address;
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

            //IPAddress ipAddress = IPAddress.Loopback;//ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(address, 2302);

            // Create a TCP/IP socket.  
            listener = new Socket(address.AddressFamily,
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

        /// <summary>
        /// If not already listening, start a callback for accepting a connection.
        /// </summary>
        public void CheckForConnections()
        {
            try
            {
                // Start an asynchronous socket to listen for connections.  
                if (!isListening)
                {
                    Console.WriteLine("Waiting for a connection...");

                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    isListening = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Connect to a requesting socket.
        /// </summary>
        /// <param name="ar">Stores socket and buffer data</param>
        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket clientSocket = listener.EndAccept(ar);

            // Create a new player and send them the world 
            SendWorldToClient(clientSocket);
            ProcessNewPlayer(clientSocket);

            // Add the new socket to the list of sockets recieving updates
            clientSockets.Add(clientSocket);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = clientSocket;
            clientSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

            //Start listening for the next connection
            listener.BeginAccept(
                new AsyncCallback(AcceptCallback),
                listener);

        }

        /// <summary>
        /// Sends all the game objects that exist within the game world to 
        /// a client 
        /// </summary>
        /// <param name="clientSocket">
        /// The client to send all the game objects in the world to 
        /// </param>
        private void SendWorldToClient(Socket clientSocket)
        {
            foreach (KeyValuePair<int, GameObjectServer> pair in GameServer.instance.gameObjectDict)
            {
                CreateObjectPacket packetToSend =
                    new CreateObjectPacket(pair.Value);
                clientSocket.Send(packetToSend.Serialize());
            }
        }

        public void SendWorldUpdateToAllClients()
        {
            List<GameObjectServer> gameObjects = GameServer.instance.gameObjectDict.Values.ToList();
            for (int i = 0; i < gameObjects.Count; i++)
            {
                Packet packetToSend = ServerPacketFactory.CreatePacket(gameObjects[i]);
                SendAll(packetToSend.Serialize());
            }
        }

        public void SendNewObjectToAll(GameObjectServer newObject)
        {
            Packet packetToSend = new CreateObjectPacket(newObject);
            SendAll(packetToSend.Serialize());
        }

        /// <summary>
        /// Creates a new player in the game, sends it out to all the clients,
        /// and then sends that active player to the clientSocket that is 
        /// specified
        /// </summary>
        /// <param name="clientSocket">
        /// The socket that needs an active player
        /// </param>
        private void ProcessNewPlayer(Socket clientSocket)
        {
            GameObject player = GameServer.instance.CreateNewPlayer();
            CreateObjectPacket setPlayerPacket =
                new CreateObjectPacket(player);
            // Create createObjectPacket, send to client
            byte[] data = setPlayerPacket.Serialize();
            Packet.Deserialize(data, out int bytes);
            Send(clientSocket, data);
        }

        /// <summary>
        /// When receiving a packet, process it.
        /// </summary>
        /// <param name="ar">Stores socket and buffer data</param>
        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesToRead = 0;
            try
            {
                // Read data from the client socket.   
                bytesToRead = handler.EndReceive(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            if (bytesToRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesToRead));

                while (bytesToRead > 0)
                {
                    Packet objectPacket =
                        Packet.Deserialize(state.buffer, out int bytesRead);
                    PlayerPackets.Add((PlayerPacket)objectPacket);
                    state.buffer = state.buffer.Skip(bytesRead).ToArray();
                    bytesToRead -= bytesRead;
                }

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
        /// Send the byteData to the socket.
        /// </summary>
        /// <param name="handler">Socket to send to</param>
        /// <param name="byteData">Data to send</param>
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

        /// <summary>
        /// Send given data to all connected sockets. 
        /// </summary>
        /// <param name="byteData">Data to send.</param>
        public void SendAll(byte[] byteData)
        {
            for (int i = 0; i < clientSockets.Count; i++)
            {
                Socket socket = clientSockets[i];

                /*if (!socket.Connected)
                {
                    Console.WriteLine("Client Disconnected");
                    //socket.Close();

                    clientSockets.Remove(socket);
                    i -= 1;
                }*/

                //If a client disconnected, close the socket and remove it from the list of sockets.
                try
                {
                    // Begin sending the data to the remote device.
                    Send(socket, byteData);
                    //socket.BeginSend(byteData, 0, byteData.Length, 0,
                     //   new AsyncCallback(SendCallback), socket);
                }

                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);

                    clientSockets.Remove(socket);
                    i -= 1;
                }
            }
        }

        /// <summary>
        /// Called when send was successful.
        /// </summary>
        /// <param name="ar">Stores socket and buffer data</param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);

                //Console.WriteLine("Sent {0} bytes to client.\n", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
