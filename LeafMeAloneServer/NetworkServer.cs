﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared;
using Shared.Packet;

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
        public List<RequestPacket> PlayerPackets = new List<RequestPacket>();

        private bool networked;
        private List<byte> ByteReceivedQueue = new List<byte>();

        //Keep track of which player is connected to each socket.
        private Dictionary<Socket, int> playerDictionary = new Dictionary<Socket, int>();

        public NetworkServer(bool networked)
        {
            this.networked = networked;
            clientSockets = new List<Socket>();
        }

        /// <summary>
        /// Open a socket for clients to conenct to.
        /// </summary>
        public void StartListening()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];
            IPAddress ipAddress = IPAddress.Loopback;
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            if (networked)
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                ipAddress = ipHostInfo.AddressList[0];
            }


            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 2302);

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
                new AsyncCallback(ReceiveCallback), state);

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
                CreateObjectPacket packetToSend = PacketFactory.NewCreatePacket(pair.Value);
                clientSocket.Send(PacketUtil.Serialize(packetToSend));
            }
        }

        public void SendWorldUpdateToAllClients()
        {
            List<GameObjectServer> gameObjects = GameServer.instance.gameObjectDict.Values.ToList();
            for (int i = 0; i < gameObjects.Count; i++)
            {
                GameObjectServer objectToSend = gameObjects[i];

                //Send an update if the object is not a leaf or if the leaf has been modified.
                if (!(objectToSend is LeafServer) || objectToSend.Modified)
                {
                    objectToSend.Modified = false;
                    BasePacket packetToSend = ServerPacketFactory.CreateUpdatePacket(gameObjects[i]);
                    SendAll(PacketUtil.Serialize(packetToSend));
                }
            }

            foreach (var gameObj in GameServer.instance.toDestroyQueue)
            {
                BasePacket packet = PacketFactory.NewDestroyPacket(gameObj);
                SendAll(PacketUtil.Serialize(packet));
            }
        }

        public void SendNewObjectToAll(GameObjectServer newObject)
        {
            BasePacket packetToSend = PacketFactory.NewCreatePacket(newObject);
            SendAll(PacketUtil.Serialize(packetToSend));
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
            PlayerServer player = GameServer.instance.CreateNewPlayer();
            CreatePlayerPacket createPlayPack = ServerPacketFactory.NewCreatePacket(player);
            // Create createObjectPacket, send to client
            byte[] data = PacketUtil.Serialize(createPlayPack);
            Send(clientSocket, data);
        }

        /// <summary>
        /// Asyncronous Receive callback. Handles recieving a new data stream and deserializing it.
        /// </summary>
        /// <param name="ar">Stores socket and buffer data</param>
        public void ReceiveCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            
            try
            {
                // Read data from the client socket.   
                int bytesReceived = handler.EndReceive(ar);


                // There might be more data, so store the data received so far.  
                lock (ByteReceivedQueue)
                {
                    ByteReceivedQueue.AddRange(state.buffer.Take(bytesReceived));
                }

                // Create a new state object for the next packet.  
                StateObject newState = new StateObject
                {
                    workSocket = handler
                };
                //Begin listening again for more packets.
                handler.BeginReceive(newState.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), newState);
            }

            catch (SocketException e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        /// <summary>
        /// Receives what it can from the byte received queue and deserializes it into the neccessary updates.
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
                PlayerPackets.Add((RequestPacket)packet);

                // Remove the read data 
                lock (ByteReceivedQueue)
                {
                    ByteReceivedQueue.RemoveRange(0, packetSize + PacketUtil.PACK_HEAD_SIZE);
                }
            }
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

                //If a client disconnected, close the socket and remove it from the list of sockets.
                try
                {
                    // Begin sending the data to the remote device.
                    Send(socket, byteData);
                }

                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Player Disconnected");

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
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
