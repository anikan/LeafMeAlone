using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    static class GraphicsManager
    {
        public static Dictionary<string, Geometry> DictGeometry = new Dictionary<string, Geometry>();
        public static Dictionary<string, Shader> DictShader = new Dictionary<string, Shader>();
        public static Dictionary<string, Light> DictLight = new Dictionary<string, Light>();
        public static Camera ActiveCamera;


    }
}
