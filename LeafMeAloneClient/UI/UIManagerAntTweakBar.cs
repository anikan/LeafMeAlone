using System;
using System.Collections.Generic;
using System.Drawing;
using AntTweakBar;

namespace Client.UI
{
    public class UIManagerAntTweakBar
    {

        //Manage active UI.
        public static Dictionary<string, Bar> ActiveUI = new Dictionary<string, Bar>();

        //Create bar.
        public static Bar Create(string name, Size size, Point location)
        {
            var b = new Bar(GraphicsRenderer.BarContext)
            {
                Label = name,
                Contained = false,
                Size = size,
                Position = location,
            };
            b.SetDefinition("refresh=.1 alpha=0");
            b.ValueColumnWidth = 0;
            if (ActiveUI.ContainsKey(name))
                throw new Exception("Invalid name!");


            ActiveUI[name] = b;
            return b;
        }
    }
}
