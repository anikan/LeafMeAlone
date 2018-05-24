using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// 
    /// </summary>
    public enum LeafState
    {
        Ignite,
        Loop,
        Putout,
        Burnup,
        Inactive
    }

    /// <summary>
    /// 
    /// </summary>
    public enum PlayerFootstepState
    {
        Loop,
        Inactive
    }

    static class AudioManager
    {
        private static AudioSystem Audio;

        private static int _srcBgm;
        private static List<int> _srcPlayerFootSteps;
        private static List<PlayerFootstepState> _playerFootstepStates;

        private static List<int> _srcFlameThrowers;
        private static List<FlameThrowerState> _flameThrowerStates;

        private static List<int> _srcLeafBlowers;
        private static List<LeafBlowerState> _leafBlowerStates;

        private static List<int> _srcLeaves;
        private static List<LeafState> _leafStates;

        public static void Init()
        {
            Audio = new AudioSystem();
            _srcBgm = Audio.GenSource();
            Audio.UpdateSourcePosition(_srcBgm, new Vector3(-10, 0, 0));
            Audio.Play(_srcBgm, @"../../Sound/song.wav", true);

            _srcFlameThrowers = new List<int>();
            _srcLeafBlowers = new List<int>();
            _srcLeaves = new List<int>();
            _srcPlayerFootSteps = new List<int>();

            _flameThrowerStates = new List<FlameThrowerState>();
            _leafBlowerStates = new List<LeafBlowerState>();
            _leafStates = new List<LeafState>();
            _playerFootstepStates = new List<PlayerFootstepState>();

        }

        public static void Update()
        {

        }
    }
}
