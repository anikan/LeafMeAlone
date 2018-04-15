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
using Buffer = SlimDX.Direct3D11.Buffer;
namespace Client
{
    class Geometry
    {
        private List<Vector3> VerticesList;
        private List<Vector3> NormalsList;
        private List<int> FacesList;
        
        /// <summary>
        /// Vertex Buffer, Index Buffer
        /// </summary>
        public Buffer VBO, EBO;

        /// <summary>
        /// Data streams hold the actual Vertices and Faces.
        /// </summary>
        protected DataStream Vertices, Faces;

        /// <summary>
        /// Assimp scene containing the loaded model.
        /// </summary>
        private Scene scene;

        /// <summary>
        /// Assimp importer.
        /// </summary>
        private AssimpContext importer;

        /// <summary>
        /// Create a new geometry given filename
        /// </summary>
        /// <param name="fileName"></param>
        public Geometry(string fileName)
        {
            VerticesList = new List<Vector3>();
            NormalsList = new List<Vector3>();
            FacesList = new List<int>();

            //Create new importer.
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
                        VerticesList.Add(vertex.ToVector3());
                    });
                    sceneMesh.Normals.ForEach(normal =>
                    {
                       NormalsList.Add(normal.ToVector3());
                    });
                    sceneMesh.Faces.ForEach(face =>
                    {
                        FacesList.AddRange(face.Indices);
                    });

                }
            }
        }
    }
}
