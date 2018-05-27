using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{

    /// <summary>
    /// A single tile of terrain in the game.
    /// </summary>
    public class MapTile : NonNetworkedGameObjectClient
    {

        // Just create the tile with the default terrain model.
        public MapTile() : base(Constants.DefaultMapModel)
        {



        }
    }
}
