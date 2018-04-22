using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Shared
{
    /// <summary>
    /// Packet of player information, to send or receive from the server.
    /// </summary>
    public class PlayerPacket : Packet
    {

        // Type of tool the player is using.
        public enum ToolType
        {

            BLOWER,
            THROWER
      
        }

        // Movement info in the packet. 
        // Note: When sending the packet, this is just a direction.
        // When receiving, this will be an absolute position.
        public Vector2 Movement;

        // Rotation of the player.
        public float Rotation;

        // If the player is actively using their tool this frame.
        public bool UsingToolPrimary;

        // If the player is using the secondary ability of their tool this frame.
        public bool UsingToolSecondary;

        // Currently equipped tool.
        public ToolType ToolEquipped;

        // Is the player dead? RIP.
        public bool Dead;

        /// <summary>
        /// Initializes a player packet, calls base constructor.
        /// </summary>
        /// <param name="id"></param>
        public PlayerPacket()
        {


        }

        /// <summary>
        /// Sends the packet to server.
        /// </summary>
        public override void Send()
        {

        }

        /// <summary>
        /// Receives a packet from the server.
        /// </summary>
        public override void Receive()
        {



        }

        public override string ToString()
        {

            string printString = string.Format("Player packet info: Movement={0}, UseToolPrimary={1}, UseToolSecondary={2}", 
                Movement, UsingToolPrimary, UsingToolSecondary);

            return printString;

        }
    }
}
