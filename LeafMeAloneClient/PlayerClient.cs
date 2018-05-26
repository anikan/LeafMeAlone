using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using Shared;
using Shared.Packet;

namespace Client
{
    /// <summary>
    /// For controlling the flamethrower source
    /// </summary>
    public enum FlameThrowerState : Byte
    {
        Start = 1,
        Loop = 2,
        End = 3,
        Inactive = 4
    }

    /// <summary>
    /// For controlling the windblower source
    /// </summary>
    public enum LeafBlowerState
    {
        Start = 1,
        Loop = 2,
        End = 3,
        Inactive = 4
    }


    /// <summary>
    /// For controlling player footstep sound
    /// </summary>
    public enum WalkingState
    {
        Loop = 1,
        Inactive = 2
    }

    public class PlayerClient : NetworkedGameObjectClient, IPlayer
    {
        // Small offset for floating point errors
        public const float FLOAT_RANGE = 0.01f;

        // Struct to contain all player info that will send via packets
        public struct PlayerRequestInfo
        {
            // Direction of movement the player is requesting. Should be 
            // between -1 and 1 each axis.
            public Vector2 MovementRequested;

            // Amount of rotation the player is requested.
            public float RotationRequested;

            // Requests for the use of primary/secondary features of tools.
            public ToolMode ActiveToolMode;

            public ToolType EquipToolRequest;
        };
        
        // All of the requests from the player that will go into a packet.
        public PlayerRequestInfo PlayerRequests;

        private ParticleSystem FlameThrower,LeafBlower;

        // For the audio control
        private int _audioFootstep, _audioFlame, _audioLeaf;
        private FlameThrowerState _throwerState;
        private LeafBlowerState _blowerState;
        private WalkingState _footstepState;

        public PlayerClient(CreateObjectPacket createPacket) : 
            base(createPacket, Constants.PlayerModel)
        {
            FlameThrower = new FlameThrowerParticleSystem();
            LeafBlower = new LeafBlowerParticleSystem();
            GraphicsManager.ParticleSystems.Add(FlameThrower);
            GraphicsManager.ParticleSystems.Add(LeafBlower);

            _audioFootstep = AudioManager.GetNewSource();
            _audioFlame = AudioManager.GetNewSource();
            _audioLeaf = AudioManager.GetNewSource();

            _throwerState = FlameThrowerState.Inactive;
            _blowerState = LeafBlowerState.Inactive;
            _footstepState = WalkingState.Inactive;
        }

        public Team team { get; set; }
        //Implementations of IPlayer fields
        public bool Dead { get; set; }
        public ToolType ToolEquipped { get; set; }
        public ToolMode ActiveToolMode { get; set; }

        /// <summary>
        /// Moves the player in a specified direction (NESW)
        /// </summary>
        /// <param name="dir"></param>
        public void RequestMove(Vector2 dir)
        {

            // If the direction requested is non-zero in the X axis 
            // (account for floating point error).
            if (dir.X < 0.0f - FLOAT_RANGE || dir.X > 0.0f + FLOAT_RANGE)
            {
                // Request in the x direction.
                PlayerRequests.MovementRequested.X = dir.X;
            }

            // If the direction requested is non-zero in the Y axis (account 
            // for floating point error).
            if (dir.Y < 0.0f - FLOAT_RANGE || dir.Y > 0.0f + FLOAT_RANGE)
            {
                //// Request in the y direction.
                PlayerRequests.MovementRequested.Y = dir.Y;
            }
        }

        /// <summary>
        /// Request the use of the primary function of the equipped tool.
        /// </summary>
        public void RequestUsePrimary()
        {
            // Set request bool to true.
            PlayerRequests.ActiveToolMode = ToolMode.PRIMARY;

        }

        /// <summary>
        /// Request the use of the secondary function of the equipped tool.
        /// </summary>
        public void RequestUseSecondary()
        {
            // Set request bool to true.
            PlayerRequests.ActiveToolMode = ToolMode.SECONDARY;
        }

        public void RequestToolEquip(ToolType type)
        {
            if (type != ToolEquipped)
            {
                Console.WriteLine("Requesting new tool! " + type.ToString());
                PlayerRequests.EquipToolRequest = type;
            }
            else
            {
                PlayerRequests.EquipToolRequest = ToolType.SAME;
            }
        }

        // Note: Causes weird behaviour sometimes. Needs to be fixed if want to use.
        public void RequestLookAtWorldSpace(Vector2 position)
        {
            Vector2 screenSize = new Vector2(GraphicsRenderer.Form.Width, GraphicsRenderer.Form.Height);

            Vector3 worldPos = GraphicsManager.ScreenToWorldPoint(position);
            Vector3 playerToPos = worldPos - Transform.Position;
            playerToPos.Y = 0.0f;

            Vector3 forward = new Vector3(0.0f, 0.0f, 1.0f);

            float dot = Vector3.Dot(playerToPos, forward);
            float mag = forward.Length() * playerToPos.Length();

            float angle = (float)Math.Acos(dot / mag);

            if (playerToPos.X < 0)
            {
                angle = -angle;
            }
       
            Transform.Rotation = new Vector3(Transform.Rotation.X, angle, Transform.Rotation.Z);

        }

        /// <summary>
        /// Requests the player to look at a specified position, using mouse on screen space calculations.
        /// </summary>
        /// <param name="position">Screenspace position to look at.</param>
        public void RequestLookAtScreenSpace(Vector2 position)
        {
            // Screen sizes
            float widthOffset = GraphicsRenderer.Form.Width / 2;
            float heightOffset = GraphicsRenderer.Form.Height / 2;

            // Up direction of the screen
            Vector3 screenUp = new Vector3(0.0f, 0.0f, -1.0f);

            // Mouse position as a Vector3. "Up" on screen is Z axis in world.
            Vector3 mousePos = new Vector3(position.X - widthOffset, 0.0f, position.Y - heightOffset);

            // Vector from the center of the screen to the mouse position.
            Vector3 centerToMouse = mousePos - Vector3.Zero;

            // Remove y component of vector.
            centerToMouse.Y = 0.0f;

            // Calculate the angle between the up (forward) vector and the center to mouse vector.
            float dotMouse = Vector3.Dot(screenUp, centerToMouse);
            float magMouse = screenUp.Length() * centerToMouse.Length();
            float angleMouse = (float)Math.Acos(dotMouse / magMouse);

            // If the mouse is on the left half of the screen, negate the angle.
            if (centerToMouse.X < 0)
            {
                angleMouse = -angleMouse;
            }

            // Get angle in degrees for easy printing.
            float angleDegrees = angleMouse * (180.0f / (float)Math.PI);

            //Console.WriteLine("Angle is: " + angleDegrees);

            PlayerRequests.RotationRequested = angleMouse;

            // Set rotation of player locally, but still set to the rotation of server later.
            Transform.Rotation = new Vector3(Transform.Rotation.X, angleMouse, Transform.Rotation.Z);

        }

        /// <summary>
        /// Set the absolute position of the player
        /// </summary>
        /// <param name="pos"></param>
        public void SetPosition(Vector2 pos)
        {
            // Set the position of the player directly

            // Update Transform on GameObject

        }

        /// <summary>
        /// Resets the player's requested movement.
        /// </summary>
        public void ResetRequests()
        {

            //  ToolType equippedTool = PlayerRequests.EquipToolRequest;

            // Reset the player requests struct to clear all info.
            PlayerRequests = new PlayerRequestInfo();
            // Set rotation initially to the rotation of the player

            PlayerRequests.RotationRequested = Transform.Rotation.Y;
  
            // PlayerRequests.EquipToolRequest = equippedTool;

        }


        /// <summary>
        /// Updates the player's values based on a received packet.
        /// </summary>
        /// <param name="packet">The packet to update from.</param>
        private void UpdateFromPacket(PlayerPacket packet)
        {
            base.UpdateFromPacket(packet.ObjData);
            Dead = packet.Dead;
            ToolEquipped = packet.ToolEquipped;
            ActiveToolMode = packet.ActiveToolMode;
            Transform.Position.Y = Constants.FLOOR_HEIGHT;

            switch (ActiveToolMode)
            {
                case ToolMode.NONE:
                    FlameThrower.EnableGeneration(false);
                    LeafBlower.EnableGeneration(false);
                    break;
                case ToolMode.PRIMARY:
                    switch (ToolEquipped)
                    {
                        case ToolType.BLOWER:
                            FlameThrower.EnableGeneration(false);
                            LeafBlower.EnableGeneration(true);
                            break;
                        case ToolType.THROWER:
                            FlameThrower.EnableGeneration(true);
                            LeafBlower.EnableGeneration(false);
                            break;
                        default:
                            break;
                    }
                    break;
                case ToolMode.SECONDARY:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            Matrix mat = Matrix.RotationX(Transform.Rotation.X) *
                         Matrix.RotationY(Transform.Rotation.Y) *
                         Matrix.RotationZ(Transform.Rotation.Z);

            FlameThrowerParticleSystem p = FlameThrower as FlameThrowerParticleSystem;
            // flame throwing particle system update
            FlameThrower.SetOrigin(Transform.Position + Vector3.TransformCoordinate(Constants.PlayerToToolOffset, mat));
            FlameThrower.SetVelocity(Transform.Forward * p.FlameInitSpeed);
            FlameThrower.SetAcceleration(Transform.Forward * p.FlameAcceleration);
            FlameThrower.Update(deltaTime);

            LeafBlower.SetOrigin(Transform.Position + Vector3.TransformCoordinate(Constants.PlayerToToolOffset, mat));
            LeafBlower.SetVelocity(Transform.Forward * p.FlameInitSpeed);
            LeafBlower.SetAcceleration(Transform.Forward * p.FlameAcceleration);
            LeafBlower.Update(deltaTime);

            _throwerState = (FlameThrowerState) AudioManager.EvaluateToolAudio(
                ToolEquipped == ToolType.THROWER && ActiveToolMode == ToolMode.PRIMARY,     // flamethrower in use?
                _audioFlame,                                                                // src that is generated by audiomanager
                (byte) _throwerState,                                                       // current thrower state
                Constants.FlameThrowerStart, Constants.FlameThrowerLoop, Constants.FlameThrowerEnd); // files to be played

            _blowerState = (LeafBlowerState) AudioManager.EvaluateToolAudio(
                ToolEquipped == ToolType.BLOWER && ActiveToolMode == ToolMode.PRIMARY,      // leafblower in use?
                _audioLeaf,                                                                 // src that is generated by audiomanager
                (byte) _blowerState,                                                        // current blower state
                Constants.LeafBlowerStart, Constants.LeafBlowerLoop, Constants.LeafBlowerEnd ); // files to be played

            _footstepState = (WalkingState) AudioManager.EvaluateLoopOnlyAudio(Moving, _audioFootstep,
                (byte) _footstepState, Constants.PlayerFootstep);

        }

        /// <summary>
        /// Facade method which calls to the actual packet processor after 
        /// casting
        /// </summary>
        /// <param name="packet">Abstract packet which gets casted to an actual 
        /// object type</param>
        public override void UpdateFromPacket(BasePacket packet)
        {
            UpdateFromPacket(packet as PlayerPacket);
        }

        /// <summary>
        /// "Kills" the player and forces a respawn.
        /// </summary>
        public override void Destroy()
        {
            // You can't destroy a player silly. Do something else here instead.
        }
    }
}
