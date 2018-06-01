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
        private int _audioFootstep, _audioFlame, _audioWind, _audioSuction;
        private int _animWalkThrower, _animWalkBlower, _animIdle, _animVictory, _animLose, _animHurt;
        private int _currAnim, _overridedAnim;

        public PlayerClient(CreateObjectPacket createPacket) : 
            base(createPacket, Constants.PlayerModel)
        {
            FlameThrower = new FlameThrowerParticleSystem();
            LeafBlower = new LeafBlowerParticleSystem();
            GraphicsManager.ParticleSystems.Add(FlameThrower);
            GraphicsManager.ParticleSystems.Add(LeafBlower);

            _audioFootstep = AudioManager.GetNewSource();
            _audioFlame = AudioManager.GetNewSource();
            _audioWind = AudioManager.GetNewSource();
            _audioSuction = AudioManager.GetNewSource();

            // Create new animations
            float scale = .07f;
            float timeScale = 3f;
            _animWalkBlower = AnimationManager.AddAnimation(Constants.PlayerWalkBlowerAnim, new Vector3(scale), timeScale);
            _animWalkThrower = AnimationManager.AddAnimation(Constants.PlayerWalkThrowerAnim, new Vector3(scale), timeScale);
            _animIdle = AnimationManager.AddAnimation(Constants.PlayerIdleAnim, new Vector3(scale), timeScale);
            _animVictory = AnimationManager.AddAnimation(Constants.PlayerVictoryAnim, new Vector3(scale), timeScale);
            _animLose = AnimationManager.AddAnimation(Constants.PlayerDefeatAnim, new Vector3(scale), timeScale);
            _animHurt = AnimationManager.AddAnimation(Constants.PlayerHurtAnim, new Vector3(scale), timeScale);

            // set to idle animation by default
            SwitchAnimation(_animIdle);
        }

        /// <summary>
        /// Get the animation variables out and set the fields
        /// </summary>
        /// <param name="animId"> ID of the animation </param>
        private void SwitchAnimation(int animId, bool repeat = true)
        {
            model = AnimationManager.GetAnimatedModel(animId, true, repeat);
            Transform.Scale = AnimationManager.GetScale(animId);

            if (team == Team.BLUE)
            {
                model.UseAltColor(new Color3(.5f, .5f, 1.0f));
            }
            else if (team == Team.RED)
            {
                model.UseAltColor(new Color3(1.0f, .5f, .5f));
            }

            _currAnim = animId;
        }

        /// <summary>
        /// Override a current animation
        /// </summary>
        /// <param name="animId"> the animation to override with </param>
        private void OverrideAnimation(int animId)
        {
            _overridedAnim = _currAnim;
            SwitchAnimation(animId, false);
        }

        /// <summary>
        /// restart the previous animation
        /// </summary>
        private void RestartOveridedAnimation()
        {
            SwitchAnimation(_overridedAnim);
            _overridedAnim = -1;
        }

        public Team team
        {
            get { return _team; }
            set
            {
                _team = value;
                model.UseAltColor( _team == Team.BLUE ? new Color3(.5f,.5f,1.0f) : new Color3(1.0f, .5f, .5f));
            }
        }

        private Team _team;

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
            bool prevMoving = Moving;
            bool prevUsingFlame = ToolEquipped == ToolType.THROWER && ActiveToolMode == ToolMode.PRIMARY;
            bool prevUsingWind = ToolEquipped == ToolType.BLOWER && ActiveToolMode == ToolMode.PRIMARY;
            bool prevUsingSuction = ToolEquipped == ToolType.BLOWER && ActiveToolMode == ToolMode.SECONDARY;
            float prevHealth = Health;
            bool prevEquipThrower = ToolEquipped == ToolType.THROWER;
            bool prevEquipBlower = ToolEquipped == ToolType.BLOWER;

            base.UpdateFromPacket(packet.ObjData);
            Dead = packet.Dead;

            if (Dead)
            {
                model.Enabled = false;
            } else
            {
                model.Enabled = true;
            }
            
            ToolEquipped = packet.ToolEquipped;
            ActiveToolMode = packet.ActiveToolMode;
            Transform.Position.Y = Constants.FLOOR_HEIGHT;

            bool currMoving = Moving;
            bool currUsingFlame = ToolEquipped == ToolType.THROWER && ActiveToolMode == ToolMode.PRIMARY;
            bool currUsingWind = ToolEquipped == ToolType.BLOWER && ActiveToolMode == ToolMode.PRIMARY;
            bool currUsingSuction = ToolEquipped == ToolType.BLOWER && ActiveToolMode == ToolMode.SECONDARY;
            bool hurt = prevHealth > Health;
            bool currEquipThrower = ToolEquipped == ToolType.THROWER;
            bool currEquipBlower = ToolEquipped == ToolType.BLOWER;


            EvaluateAnimation(prevMoving, currMoving, prevEquipBlower, currEquipBlower, prevEquipThrower, currEquipThrower, hurt);
            EvaluateAudio(prevMoving, currMoving, prevUsingFlame, currUsingFlame, prevUsingWind, currUsingWind, prevUsingSuction, currUsingSuction);

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

        /// <summary>
        /// Evaluate which animation model to use
        /// </summary>
        /// <param name="prevMoving"> is player moving previously? </param>
        /// <param name="currMoving"> is player moving currently? </param>
        /// <param name="prevEquipBlower"> is the leaf blower equipped previously? </param>
        /// <param name="currEquipBlower"> is the leaf blower equipped currently? </param>
        /// <param name="prevEquipThrower"> is the flame thrower equipped previously? </param>
        /// <param name="currEquipThrower"> is the flame thrower equipped currently? </param>
        /// <param name="hurt"> is the player hurt? </param>
        private void EvaluateAnimation(bool prevMoving, bool currMoving, bool prevEquipBlower, bool currEquipBlower, bool prevEquipThrower, bool currEquipThrower, bool hurt)
        {
            if (prevMoving && !currMoving)
            {
                SwitchAnimation(_animIdle);
            }
            else if (!prevMoving && currMoving)
            {
                if (currEquipBlower)
                {
                    SwitchAnimation(_animWalkBlower);
                }
                else if (currEquipThrower)
                {
                    SwitchAnimation(_animWalkThrower);
                }
            }
            else if (currMoving && !prevEquipBlower && currEquipBlower)
            {
                SwitchAnimation(_animWalkBlower);
            }
            else if (currMoving && !prevEquipThrower && currEquipThrower)
            {
                SwitchAnimation(_animWalkThrower);
            }
            
        }

        /// <summary>
        /// Evaluate audio logic
        /// </summary>
        /// <param name="prevMoving"> moving previously? </param>
        /// <param name="currMoving"> moving currently? </param>
        /// <param name="prevUsingFlame"> using flamethrower previously? </param>
        /// <param name="currUsingFlame">using flamethrower currently? </param>
        /// <param name="prevUsingWind"> using windblower previously? </param>
        /// <param name="currUsingWind"> using windblower currently? </param>
        /// <param name="prevUsingSuction"> using suction previously? </param>
        /// <param name="currUsingSuction"> using suction currently? </param>
        public void EvaluateAudio(bool prevMoving, bool currMoving, 
            bool prevUsingFlame, bool currUsingFlame, 
            bool prevUsingWind, bool currUsingWind,
            bool prevUsingSuction, bool currUsingSuction)
        {
            AudioManager.UpdateSourceLocation(_audioFlame, Transform.Position);
            AudioManager.UpdateSourceLocation(_audioWind, Transform.Position);
            AudioManager.UpdateSourceLocation(_audioFootstep, Transform.Position);
            AudioManager.UpdateSourceLocation(_audioSuction, Transform.Position);

            // footstep audio logic
            // if start moving
            if (!prevMoving && currMoving)
            {
                AudioManager.PlayAudio(_audioFootstep, Constants.PlayerFootstep, true);
            }
            // if stop moving
            else if (prevMoving && !currMoving)
            {
                AudioManager.StopAudio(_audioFootstep);
            }

            // flamethrower audio logic
            // if start using flame now
            if (!prevUsingFlame && currUsingFlame)
            {
                AudioManager.StopAudio(_audioFlame);
                AudioManager.QueueAudioToSource(_audioFlame, Constants.FlameThrowerStart, false);
                AudioManager.QueueAudioToSource(_audioFlame, Constants.FlameThrowerLoop, true);
            }
            // if stop using flame now
            else if (prevUsingFlame && !currUsingFlame)
            {
                AudioManager.StopAudio(_audioFlame);
                AudioManager.PlayAudio(_audioFlame, Constants.FlameThrowerEnd, false);
            }

            // windblower audio logic
            // if start using wind now
            if (!prevUsingWind && currUsingWind)
            {
                AudioManager.StopAudio(_audioWind);
                AudioManager.QueueAudioToSource(_audioWind, Constants.LeafBlowerStart, false);
                AudioManager.QueueAudioToSource(_audioWind, Constants.LeafBlowerLoop, true);
            }
            // if stop using wind now
            else if (prevUsingWind && !currUsingWind)
            {
                AudioManager.StopAudio(_audioWind);
                AudioManager.PlayAudio(_audioWind, Constants.LeafBlowerEnd, false);
            }

            // suction audio logic
            if (!prevUsingSuction && currUsingSuction)
            {
                AudioManager.StopAudio(_audioSuction);
                AudioManager.QueueAudioToSource(_audioSuction, Constants.SuctionStart, false);
                AudioManager.QueueAudioToSource(_audioSuction, Constants.SuctionLoop, true);
            }
            else if (prevUsingSuction && !currUsingSuction)
            {
                AudioManager.StopAudio(_audioSuction);
                AudioManager.PlayAudio(_audioSuction, Constants.SuctionEnd, false);
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
    }
}
