using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.UI;
using SlimDX;
using Shared;
using Shared.Packet;
using System.Diagnostics;

namespace Client
{
    public class PlayerClient : NetworkedGameObjectClient, IPlayer
    {
        // Small offset for floating point errors
        public const float FLOAT_RANGE = 0.01f;
        public const float SUCTION_SPEED = 80f;

        public bool StoppedRequesting = true;

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

        private NormalParticleSystem FlameThrower, LeafBlower;
        private InverseParticleSystem Suction;

        // For the audio control
        private int _audioFootstep, _audioFlame, _audioWind, _audioSuction, _audioVoice;
        private int _animWalkThrower, _animWalkBlower, _animIdle, _animVictory, _animLose;

        private int _currAnim, _overridedAnim;
        public UIHealth healthUI;
        public UINickname nicknameUI;

        public PlayerStats stats;

        

        public PlayerClient(CreateObjectPacket createPacket) :
            base(createPacket, Constants.PlayerModel)
        {
            FlameThrower = new FlameThrowerParticleSystem(Tool.Thrower.ConeAngle * 10f, 40.0f, 15.0f, Tool.Thrower.Range/2.0f, 1.0f, Tool.Thrower.Range, 1.0f);
            LeafBlower = new LeafBlowerParticleSystem();
            Suction = new InverseParticleSystem(Constants.WindTexture, Vector3.Zero, -Vector3.UnitX*SUCTION_SPEED, true);
            GraphicsManager.ParticleSystems.Add(FlameThrower);
            GraphicsManager.ParticleSystems.Add(LeafBlower);
            GraphicsManager.ParticleSystems.Add(Suction);
            FlameThrower.EnableGeneration(false);
            LeafBlower.EnableGeneration(false);
            Suction.EnableGeneration(false);
            
            _audioFootstep = AudioManager.GetNewSource();
            _audioFlame = AudioManager.GetNewSource();
            _audioWind = AudioManager.GetNewSource();
            _audioSuction = AudioManager.GetNewSource();
            _audioVoice = AudioManager.GetNewSource();

            // Create new animations
            float scale = .07f;
            float timeScale = 3f;
            _animWalkBlower = AnimationManager.AddAnimation(Constants.PlayerWalkBlowerAnim, new Vector3(scale), timeScale);
            _animWalkThrower = AnimationManager.AddAnimation(Constants.PlayerWalkThrowerAnim, new Vector3(scale), timeScale);
            _animIdle = AnimationManager.AddAnimation(Constants.PlayerIdleAnim, new Vector3(scale), timeScale);
            _animVictory = AnimationManager.AddAnimation(Constants.PlayerVictoryAnim, new Vector3(scale), timeScale);
            _animLose = AnimationManager.AddAnimation(Constants.PlayerDefeatAnim, new Vector3(scale), timeScale);
            
            // set to idle animation by default
            SwitchAnimation(_animIdle);
            nicknameUI = new UINickname(this,Name);

            Burnable = true;
        }

        /// <summary>
        /// Get the animation variables out and set the fields
        /// </summary>
        /// <param name="animId"> ID of the animation </param>
        private void SwitchAnimation(int animId, bool repeat = true, int index = 0)
        {
            if (_currAnim == animId) return;

            model = AnimationManager.GetAnimatedModel(animId, repeat, false, index);
            Transform.Scale = AnimationManager.GetScale(animId);

            if (PlayerTeam == TeamName.BLUE)
            {
                model.UseAltColor(new Color3(.4f, .4f, 1.2f));
            }
            else if (PlayerTeam == TeamName.RED)
            {
                model.UseAltColor(new Color3(1.2f, .4f, .4f));
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

        public TeamName PlayerTeam
        {
            get => _team;
            set
            {
                _team = value;

                model.UseAltColor( _team == TeamName.BLUE ? new Color3(.4f,.4f,1.2f) : new Color3(1.2f, .4f, .4f));
            }
        }

        private TeamName _team;

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

        /// <summary>
        /// Request to equip a specific tool type.
        /// </summary>
        /// <param name="type">Tool type to equip.</param>
        public void RequestToolEquip(ToolType type)
        {
            // If this isn't the existing tool type.
            if (type != ToolEquipped)
            {
                // Request the new tool.
                PlayerRequests.EquipToolRequest = type;
            }
            // If it's the same as what's equipped.
            else
            {
                // Just request same.
                PlayerRequests.EquipToolRequest = ToolType.SAME;
            }
        }

        /// <summary>
        /// Request to cycle to the next tool.
        /// </summary>
        public void RequestCycleTool()
        {

            // If we don't already have a request and check the bool to prevent rapid cycling.
            if (PlayerRequests.EquipToolRequest == ToolType.SAME && StoppedRequesting)
            {

                // We have not stopped requesting.
                StoppedRequesting = false;

                // If the equipped tool is the blower.
                if (ToolEquipped == ToolType.BLOWER)
                {
                    // Equip the thrower.
                    PlayerRequests.EquipToolRequest = ToolType.THROWER;
                }

                //If the equipped tool is the thrower.
                else
                {
                    // Equip the blower.
                    PlayerRequests.EquipToolRequest = ToolType.BLOWER;
                }
            }
        }

        /// <summary>
        /// Gets the transform of the active tool.
        /// </summary>
        /// <returns>Transform of the tool.</returns>
        public Transform GetToolTransform()
        {
            Matrix mat = Matrix.RotationX(Transform.Rotation.X) *
                       Matrix.RotationY(Transform.Rotation.Y) *
                       Matrix.RotationZ(Transform.Rotation.Z);

            Transform toolTransform = new Transform();
            toolTransform.Position = Transform.Position + Vector3.TransformCoordinate(Constants.PlayerToToolOffset, mat);
            toolTransform.Rotation = Transform.Rotation;

            // TODO: Make this the actual tool transform.
            // Currently just the player transform.

            return toolTransform;

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
        /// Tints all object sin the player's range.
        /// </summary>
        public void TintObjectsInRange()
        {

            // Get the networked objects.
            foreach (NetworkedGameObjectClient obj in GameClient.instance.NetworkedGameObjects.Values)
            {

                // If leaf or player.
                if (obj is LeafClient)
                {

                    // If it's within the tool range.
                    if (obj.IsWithinToolRange(GetToolTransform(), ToolEquipped, ActiveToolMode))
                    {

                        // If we haven't already modified this object.
                        if (!modifiedHue)
                        {
                            // Increase the hue.
                            obj.CurrentHue += Constants.SELECTION_HUE;

                            // Now modified.
                            obj.modifiedHue = true;
                        }
                    }

                    // If it's not within range and it's hue has been modified, reset it.
                    else if (modifiedHue)
                    {
                        obj.CurrentHue -= Constants.SELECTION_HUE;
                    }
                }
            }
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
            bool prevDeath = Health <= 0;

            base.UpdateFromPacket(packet.ObjData);

            if(GraphicsManager.ActivePlayer != this)
                Name = packet.Name ?? "";

            //Set the player color based on health.
            CurrentTint = new Vector3(1, 1, 1) * ((Health / Constants.PLAYER_HEALTH) * .7f + .3f);

            // If death state changes, reset tint.
            if (Dead != packet.Dead)
            {
                CurrentTint = new Vector3(1, 1, 1);
                CurrentHue = new Vector3(1, 1, 1);
            }

            Dead = packet.Dead;

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
            bool currDeath = Health <= 0;

            EvaluateAnimation(prevMoving, currMoving, prevEquipBlower, currEquipBlower, prevEquipThrower, currEquipThrower);
            EvaluateAudio(prevMoving, currMoving, prevUsingFlame, currUsingFlame, prevUsingWind, currUsingWind, prevUsingSuction, currUsingSuction, hurt, prevDeath, currDeath);

            // Depending on death state, show model
            if (Dead)
            {
                model.Enabled = false;
                if (healthUI != null)
                    healthUI.UITexture.Enabled = false;
                nicknameUI.enabled = false;
            }
            else
            {
                model.Enabled = true;
                if (healthUI != null)
                    healthUI.UITexture.Enabled = true;
                nicknameUI.enabled = true;
            }

            FlameThrower.EnableGeneration(false);
            LeafBlower.EnableGeneration(false);
            Suction.EnableGeneration(false);

            switch (ActiveToolMode)
            {
                case ToolMode.NONE:
                    break;
                case ToolMode.PRIMARY:
                    switch (ToolEquipped)
                    {
                        case ToolType.BLOWER:
                            LeafBlower.EnableGeneration(true);
                            break;
                        case ToolType.THROWER:
                            FlameThrower.EnableGeneration(true);
                            break;
                        default:
                            break;
                    }
                    break;
                case ToolMode.SECONDARY:
                    switch (ToolEquipped)
                    {
                        case ToolType.BLOWER:
                            Suction.EnableGeneration(true);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private const float HURT_LAG = 2f;
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
        private void EvaluateAnimation(bool prevMoving, bool currMoving, bool prevEquipBlower, bool currEquipBlower, bool prevEquipThrower, bool currEquipThrower)
        {
            // win/lose animation
            if (GameClient.instance.PendingRematchState)
            {
                if (GameClient.instance.WinningTeam == _team)
                {
                    SwitchAnimation(_animVictory);
                }
                else
                {
                    SwitchAnimation(_animLose);
                }
            }
            else if (!currMoving)
            {
                SwitchAnimation(_animIdle);
            }
            else if (currEquipBlower)
            {
                SwitchAnimation(_animWalkBlower);
            }
            else if (currEquipThrower)
            {
                SwitchAnimation(_animWalkThrower);
            }
            
        }

        private bool playedEndGameVoice = false;
        private bool playedDeathVoice = false;
        private float _hurtTimer = 0f;
        public bool PlayingHurtVoice = false;

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
            bool prevUsingSuction, bool currUsingSuction, bool hurt,
            bool prevDeath, bool currDeath)
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

            // Squirrel voice audio logic
            if (GameClient.instance.PendingRematchState)
            {
                if (!playedEndGameVoice)
                {
                    if (_team == GameClient.instance.WinningTeam)
                    {
                        AudioManager.PlayAudio(_audioVoice, Constants.SqVoiceVictory, true);

                    }
                    else
                    {
                        AudioManager.PlayAudio(_audioVoice, Constants.SqVoiceDefeat, true);

                    }
                    playedEndGameVoice = true;
                }
            }
            else
            {
                if (!Dead) playedDeathVoice = false;
                if (playedEndGameVoice) AudioManager.StopAudio(_audioVoice);
                playedEndGameVoice = false;
                PlayingHurtVoice = false;

                if (Dead && !playedDeathVoice)
                {
                    AudioManager.PlayAudio(_audioVoice, Constants.SqVoiceDeath, false);
                    playedDeathVoice = true;
                }
                else if (hurt)
                {
                    if (!AudioManager.IsSourcePlaying(_audioVoice) && _hurtTimer > 0.6f)
                    {
                        _hurtTimer = 0f;
                        AudioManager.PlayAudio(_audioVoice, Constants.SqVoiceHurt, false);

                    }

                    PlayingHurtVoice = AudioManager.IsSourcePlaying(_audioVoice);
                    
                }
                else if (!prevUsingFlame && currUsingFlame)
                {
                    AudioManager.PlayAudio(_audioVoice, Constants.SqVoiceFlameLaugh, false);
                }


            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            Matrix mat = Matrix.RotationX(Transform.Rotation.X) *
                         Matrix.RotationY(Transform.Rotation.Y) *
                         Matrix.RotationZ(Transform.Rotation.Z);

            Transform toolTransform = GetToolTransform();

            FlameThrowerParticleSystem p = FlameThrower as FlameThrowerParticleSystem;
            // flame throwing particle system update
            FlameThrower.SetOrigin(toolTransform.Position);
            FlameThrower.SetVelocity(Transform.Forward * p.FlameInitSpeed);
            FlameThrower.SetAcceleration(Transform.Forward * p.FlameAcceleration);
            FlameThrower.Update(deltaTime);

            LeafBlower.SetOrigin(toolTransform.Position);
            LeafBlower.SetVelocity(Transform.Forward * p.FlameInitSpeed);
            LeafBlower.SetAcceleration(Transform.Forward * p.FlameAcceleration);
            LeafBlower.Update(deltaTime);

            Suction.SetEndposition(toolTransform.Position);
            Suction.SetVelocity(-Transform.Forward * SUCTION_SPEED);
            Suction.Update(deltaTime);

            if (!PlayingHurtVoice) _hurtTimer += deltaTime;
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

        public override void Draw()
        {


            base.Draw();
            //if the object is currently burning, draw the fire on them.
            if (Burning)
            {
                Transform t = new Transform { Position = Transform.Position + new Vector3(0, 9, 0), Scale = new Vector3(1, 1, 1) };
                GraphicsManager.DrawParticlesThisFrame(Fire, t);
            }

            if (healthUI == null)
                healthUI = new UIHealth(this, PlayerTeam);
            //if (nicknameUI == null)
            //    nicknameUI = new UINickname(this, Name);
            healthUI?.Update();
            nicknameUI?.Update();

        }

        public override void Die()
        {
            healthUI.UITexture.Enabled = false;
            healthUI = null;
            nicknameUI.enabled = false;
            base.Die();
        }
    }
}
