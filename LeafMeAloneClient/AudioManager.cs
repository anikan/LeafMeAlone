using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    /// <summary>
    /// For controlling the flamethrower source
    /// </summary>
    public enum FlameThrowerState
    {
        Start,
        Loop,
        End,
        Inactive
    }

    /// <summary>
    /// For controlling the windblower source
    /// </summary>
    public enum LeafBlowerState
    {
        Start,
        Loop,
        End,
        Inactive
    }

    /// <summary>
    /// For controlling the burning sound of leaves
    /// </summary>
    public enum LeafState
    {
        Ignite,
        Loop,
        Putoff,
        Burnup,
        Inactive
    }

    /// <summary>
    /// For controlling player footstep sound
    /// </summary>
    public enum PlayerFootstepState
    {
        Loop,
        Inactive
    }

    /// <summary>
    /// Manages all audio stuffs with a singleton audio system
    /// </summary>
    static class AudioManager
    {
        private static AudioSystem _audio;

        private static int _srcBgm;

        private static List<PlayerClient> _allPlayerRef;
        private static List<LeafClient> _allLeafRef;

        private static int _playerCount;
        private static List<int> _srcPlayerFootSteps;
        private static List<PlayerFootstepState> _playerFootstepStates;
        private static List<Vector3> _playerPositions;

        private static List<int> _srcFlameThrowers;
        private static List<FlameThrowerState> _flameThrowerStates;

        private static List<int> _srcLeafBlowers;
        private static List<LeafBlowerState> _leafBlowerStates;

        private const int MAX_LEAF_SOURCE = 50;
        private static int _leafCount;
        private static List<int> _srcLeaves;
        private static List<bool> _leafSrcInUse;
        private static Dictionary<int, int> _leafSrcIndex;  // map from index of _allLeafRef to index of _srcLeaves
        private static List<LeafState> _leafStates;

        /// <summary>
        /// Initialize fields
        /// </summary>
        public static void Init()
        {
            _audio = new AudioSystem();
            _srcBgm = _audio.GenSource();

            _srcFlameThrowers = new List<int>();
            _srcLeafBlowers = new List<int>();
            _srcLeaves = new List<int>();
            _srcPlayerFootSteps = new List<int>();

            _flameThrowerStates = new List<FlameThrowerState>();
            _leafBlowerStates = new List<LeafBlowerState>();
            _leafStates = new List<LeafState>();
            _playerFootstepStates = new List<PlayerFootstepState>();

            _leafSrcInUse = new List<bool>();
            _leafSrcIndex = new Dictionary<int, int>();
            _allPlayerRef = new List<PlayerClient>();
            _allLeafRef = new List<LeafClient>();
            _playerPositions = new List<Vector3>();

            _playerCount = 0;
            _leafCount = 0;
        }

        private const int MAX_FRAME_PER_UDPATE = 15;

        /// <summary>
        /// Update audio effects
        /// </summary>
        public static void Update()
        {
            // update the listener orientation
            _audio.UpdateListener(GraphicsManager.ActiveCamera.CameraPosition, GraphicsManager.ActiveCamera.CameraLookAt,
                GraphicsManager.ActiveCamera.CameraUp);

            for(int i = 0; i < _playerCount; i++)
            {

                //moved = ! _playerPositions[i].Equals(_allPlayerRef[i].Transform.Position);
                bool moved = _allPlayerRef[i].Moving;
                if (moved)
                {
                    _audio.UpdateSourcePosition(_srcPlayerFootSteps[i], _allPlayerRef[i].Transform.Position);
                    _audio.UpdateSourcePosition(_srcFlameThrowers[i], _allPlayerRef[i].Transform.Position);
                    _audio.UpdateSourcePosition(_srcLeafBlowers[i], _allPlayerRef[i].Transform.Position);
                }

                bool throwerEquipped = _allPlayerRef[i].ToolEquipped == ToolType.THROWER;
                bool blowerEquipped = _allPlayerRef[i].ToolEquipped == ToolType.BLOWER;
                bool toolPrimaryInUse = _allPlayerRef[i].ActiveToolMode == ToolMode.PRIMARY;

                // Control player footstep audio
                switch (_playerFootstepStates[i])
                {
                    case PlayerFootstepState.Inactive:
                    {
                        if (moved)
                        {
                            _playerFootstepStates[i] = PlayerFootstepState.Loop;
                            _audio.Play( _srcPlayerFootSteps[i], Constants.PlayerFootstep, true );
                        }
                        break;
                    }
                    case PlayerFootstepState.Loop:
                    {
                        if (!moved)
                        {
                            _playerFootstepStates[i] = PlayerFootstepState.Inactive;
                            _audio.Stop( _srcPlayerFootSteps[i] );
                        }
                        break;
                    }
                }

                // Control flamethrower audio
                switch (_flameThrowerStates[i])
                {
                    case FlameThrowerState.Inactive:
                    {
                        if (throwerEquipped && toolPrimaryInUse)
                        {
                            _flameThrowerStates[i] = FlameThrowerState.Start;
                            _audio.Play(_srcFlameThrowers[i], Constants.FlameThrowerStart, false);
                        }
                        break;
                    }
                    case FlameThrowerState.Start:
                    {
                        if (throwerEquipped && toolPrimaryInUse)
                        {
                            if (! _audio.IsPlaying( _srcFlameThrowers[i]) )
                            {
                                _audio.Play(_srcFlameThrowers[i], Constants.FlameThrowerLoop, true);
                                _flameThrowerStates[i] = FlameThrowerState.Loop;
                            }
                        }
                        else
                        {
                            _audio.Play(_srcFlameThrowers[i], Constants.FlameThrowerEnd, false);
                            _flameThrowerStates[i] = FlameThrowerState.End;
                        }
                        break;
                    }
                    case FlameThrowerState.Loop:
                    {
                        if (!(throwerEquipped && toolPrimaryInUse))
                        {
                            _audio.Play(_srcFlameThrowers[i], Constants.FlameThrowerEnd, false);
                            _flameThrowerStates[i] = FlameThrowerState.End;
                        }
                        break;
                    }
                    case FlameThrowerState.End:
                    {
                        if (!_audio.IsPlaying(_srcFlameThrowers[i]) )
                        {
                            _flameThrowerStates[i] = FlameThrowerState.Inactive;
                        }
                        else if (throwerEquipped && toolPrimaryInUse)
                        {
                            _audio.Play( _srcFlameThrowers[i], Constants.FlameThrowerStart, false);
                            _flameThrowerStates[i] = FlameThrowerState.Start;
                        }
                        break;
                    }
                }
                
                // control leaf blower audio
                switch (_leafBlowerStates[i])
                {
                    case LeafBlowerState.Inactive:
                    {
                        if (blowerEquipped && toolPrimaryInUse)
                        {
                            _leafBlowerStates[i] = LeafBlowerState.Start;
                            _audio.Play(_srcLeafBlowers[i], Constants.LeafBlowerStart, false);
                        }
                        break;
                    }
                    case LeafBlowerState.Start:
                    {
                        if (blowerEquipped && toolPrimaryInUse)
                        {
                            if (!_audio.IsPlaying(_srcLeafBlowers[i]))
                            {
                                _audio.Play(_srcLeafBlowers[i], Constants.LeafBlowerLoop, true);
                                _leafBlowerStates[i] = LeafBlowerState.Loop;
                            }
                        }
                        else
                        {
                            _audio.Play(_srcLeafBlowers[i], Constants.LeafBlowerEnd, false);
                            _leafBlowerStates[i] = LeafBlowerState.End;
                        }
                        break;
                    }
                    case LeafBlowerState.Loop:
                    {
                        if (!(blowerEquipped && toolPrimaryInUse))
                        {
                            _audio.Play(_srcLeafBlowers[i], Constants.LeafBlowerEnd, false);
                            _leafBlowerStates[i] = LeafBlowerState.End;
                        }
                        break;
                    }
                    case LeafBlowerState.End:
                    {
                        if (!_audio.IsPlaying(_srcLeafBlowers[i]))
                        {
                            _leafBlowerStates[i] = LeafBlowerState.Inactive;
                        }
                        else if (blowerEquipped && toolPrimaryInUse)
                        {
                            _audio.Play(_srcLeafBlowers[i], Constants.LeafBlowerStart, false);
                            _leafBlowerStates[i] = LeafBlowerState.Start;
                        }
                        break;
                    }
                }
            }
            
            for (int i = 0; i < _allLeafRef.Count; i++)
            {
                if (_leafSrcIndex[i] != -1)
                {
                    int leafIdx = _leafSrcIndex[i];
                    _audio.UpdateSourcePosition( _srcLeaves[leafIdx], _allLeafRef[i].Transform.Position);

                    switch (_leafStates[leafIdx])
                    {
                        case LeafState.Ignite: 
                        {
                            if (!_audio.IsPlaying(_srcLeaves[leafIdx]))
                            {
                                _audio.Play(_srcLeaves[leafIdx], Constants.LeafBurning, true);
                                _leafStates[leafIdx] = LeafState.Loop;
                            }
                            else if (_allLeafRef[i].Health < 0)
                            {
                                _audio.Play(_srcLeaves[leafIdx], Constants.LeafBurnup, false);
                                _leafStates[leafIdx] = LeafState.Burnup;
                            }
                            else if (!_allLeafRef[i].Burning)
                            {
                                _audio.Play(_srcLeaves[leafIdx], Constants.LeafPutoff, false);
                                _leafStates[leafIdx] = LeafState.Putoff;
                            }
                            break;
                        }
                        case LeafState.Loop:
                        {
                            if (_allLeafRef[i].Health < 0)
                            {
                                _audio.Play(_srcLeaves[leafIdx], Constants.LeafBurnup, false);
                                _leafStates[leafIdx] = LeafState.Burnup;
                            }
                            else if (!_allLeafRef[i].Burning)
                            {
                                _audio.Play(_srcLeaves[leafIdx], Constants.LeafPutoff, false);
                                _leafStates[leafIdx] = LeafState.Putoff;
                            }
                            break;
                        }
                        case LeafState.Putoff:
                        {
                            if (!_audio.IsPlaying(_srcLeaves[leafIdx]))
                            {
                                _leafSrcInUse[leafIdx] = false;
                                _leafSrcIndex[i] = -1;
                            }
                            break;
                        }
                        case LeafState.Burnup:
                        {
                            if (!_audio.IsPlaying(_srcLeaves[leafIdx]))
                            {
                                _leafSrcInUse[leafIdx] = false;
                                _leafSrcIndex[i] = -1;
                            }
                            break;
                        }
                    }

                }

                // make a new source ignite
                else if (_allLeafRef[i].Burning)
                {
                    int leafidx = _leafSrcInUse.IndexOf(false);
                    if (leafidx < 0)
                    {
                        break;
                    }
                    _leafSrcIndex[i] = leafidx;
                    _leafSrcInUse[leafidx] = true;

                    _audio.Play(_srcLeaves[leafidx], Constants.LeafIgniting, false);
                    _leafStates[leafidx] = LeafState.Ignite;
                }
            }
        }

        /// <summary>
        /// Add a new player 
        /// </summary>
        /// <param name="player"> player to be added </param>
        public static void AddPlayerSource(PlayerClient player)
        {
            _playerCount++;
            _allPlayerRef.Add( player );
            _srcPlayerFootSteps.Add( _audio.GenSource() );
            _srcLeafBlowers.Add( _audio.GenSource() );
            _srcFlameThrowers.Add( _audio.GenSource() );
            _playerFootstepStates.Add( PlayerFootstepState.Inactive );
            _leafBlowerStates.Add( LeafBlowerState.Inactive );
            _flameThrowerStates.Add( FlameThrowerState.Inactive );

            Vector3 temp;
            _playerPositions.Add( (temp = new Vector3()) );
            temp.Copy(player.Transform.Position);
        }

        /// <summary>
        /// Add a new leaf source 
        /// </summary>
        /// <param name="leaf"> the leaf to be added </param>
        public static void AddLeafSource(LeafClient leaf)
        {
            _allLeafRef.Add(leaf);
            _leafSrcIndex[_leafCount] = -1;
            if (++_leafCount > MAX_LEAF_SOURCE) return;

            _srcLeaves.Add( _audio.GenSource() );
            _leafStates.Add( LeafState.Inactive );
            _leafSrcInUse.Add( false );
        }

        /// <summary>
        /// Start or stop playing the BGM
        /// </summary>
        /// <param name="enableBGM"> indicate to start or stop the BGM</param>
        public static void PlayBGM(bool enableBGM)
        {
            if (enableBGM)
            {
                _audio.Play(_srcBgm, Constants.Bgm, true);
            }
            else
            {
                _audio.Stop(_srcBgm);
            }
        }
        
    }
}
