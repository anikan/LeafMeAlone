using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Client
{
    class ViewFrustum
    {
        //List of planes to make up the view frustum.
        private readonly List<Plane> frustum = new List<Plane>();

        /// <summary>
        /// Constructor which takes in the view and projection matrices.
        /// </summary>
        public ViewFrustum(Matrix view, Matrix proj)
        {
            //get view proj matrix.
            Matrix viewProj = view * proj;

            //Add all the eplanes
            frustum.Add(new Plane(viewProj.M14 + viewProj.M11, viewProj.M24 + viewProj.M21, viewProj.M34 + viewProj.M31, 
                viewProj.M44 + viewProj.M41));
            frustum.Add(new Plane(viewProj.M14 - viewProj.M11, viewProj.M24 - viewProj.M21, viewProj.M34 - viewProj.M31,
                viewProj.M44 - viewProj.M41));
            frustum.Add(new Plane(viewProj.M14 - viewProj.M12, viewProj.M24 - viewProj.M22, viewProj.M34 - viewProj.M32,
                viewProj.M44 - viewProj.M42));
            frustum.Add(new Plane(viewProj.M14 + viewProj.M12, viewProj.M24 + viewProj.M22, viewProj.M34 + viewProj.M32,
                viewProj.M44 + viewProj.M42));
            frustum.Add(new Plane(viewProj.M13, viewProj.M23, viewProj.M33, viewProj.M43));
            frustum.Add(new Plane(viewProj.M14 - viewProj.M13, viewProj.M24 - viewProj.M23, viewProj.M34 - viewProj.M33,
                viewProj.M44 - viewProj.M43));

            //normalize the planes.
            foreach (Plane plane in frustum)
            {
                plane.Normalize();
            }
        }

        /// <summary>
        /// Get the intersection of objects with the frustum.
        /// </summary>
        /// <param name="box"></param>
        /// <returns>
        /// Returns 0 if completely outside frustum.
        /// Returns 1 if intersection with frustum. 
        /// Returns 2 if completely inside frustum.
        /// </returns>
        public int Intersect(BoundingBox box)
        {
            //number of planes the object intersects with
            int numIntersections = 0;

            foreach (Plane plane in frustum)
            {

                //Check intersections.
                PlaneIntersectionType intersection = Plane.Intersects(plane, box);

                //if the intersection is with the back fo the plane, that means the object is outside.
                if (intersection == PlaneIntersectionType.Back) return 0;

                //if in the front of the plane, the object is inside.
                if (intersection == PlaneIntersectionType.Front)
                {
                    numIntersections++;
                }
            }

            //since we have 6 planes, if numIntersections is 6 then it is completely inside. Otherwise return 1 for partial.
            return numIntersections == 6 ? 2 : 1;
        }
    }
}
