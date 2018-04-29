using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    class LeafClient : GameObjectClient, ILeaf
    {
        public bool Burning { get; set; }
        public float TimeBurning { get; set; }

        public const string LeafModelPath = @"../../Models/LeafModel.fbx";

        public LeafClient() : base(LeafModelPath)
        {

        }

        public LeafClient(Transform startTransform) : base(LeafModelPath, startTransform)
        {

        }
    }
}
