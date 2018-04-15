using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using Assimp.Configs;
using Shared;
using SlimDX;
namespace Client
{
    class Geometry
    {
        private List<Vector3> Vertices;
        private List<Vector3> Normals;
        private List<int> Faces;

        private Scene scene;


        private AssimpContext importer;


        public Geometry(string fileName)
        {
            importer = new AssimpContext();
            scene = importer.ImportFile(fileName);
            if (scene == null)
            {
                throw new FileNotFoundException();
            }
            else
            {
                foreach (Mesh sceneMesh in scene.Meshes)
                {
                    sceneMesh.Vertices.ForEach(vertex =>
                    {
                        Vertices.Add(vertex.ToVector3());
                    });
                }
            }
        }
    }
}
