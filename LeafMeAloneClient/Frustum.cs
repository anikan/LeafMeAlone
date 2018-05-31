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
        private List<Plane> Frustum = new List<Plane>();

        public ViewFrustum(Matrix View, Matrix Proj)
        {
            var ViewProj = View * Proj;
            Frustum.Add(new Plane(ViewProj.M14 + ViewProj.M11, ViewProj.M24 + ViewProj.M21, ViewProj.M34 + ViewProj.M31, 
                ViewProj.M44 + ViewProj.M41));
            Frustum.Add(new Plane(ViewProj.M14 - ViewProj.M11, ViewProj.M24 - ViewProj.M21, ViewProj.M34 - ViewProj.M31,
                ViewProj.M44 - ViewProj.M41));
            Frustum.Add(new Plane(ViewProj.M14 - ViewProj.M12, ViewProj.M24 - ViewProj.M22, ViewProj.M34 - ViewProj.M32,
                ViewProj.M44 - ViewProj.M42));
            Frustum.Add(new Plane(ViewProj.M14 + ViewProj.M12, ViewProj.M24 + ViewProj.M22, ViewProj.M34 + ViewProj.M32,
                ViewProj.M44 + ViewProj.M42));
            Frustum.Add(new Plane(ViewProj.M13, ViewProj.M23, ViewProj.M33, ViewProj.M43));
            Frustum.Add(new Plane(ViewProj.M14 - ViewProj.M13, ViewProj.M24 - ViewProj.M23, ViewProj.M34 - ViewProj.M33,
                ViewProj.M44 - ViewProj.M43));

            foreach (Plane plane in Frustum)
            {
                plane.Normalize();
            }
        }
        public int Intersect(BoundingBox box)
        {
            var totalIn = 0;

            foreach (var plane in Frustum)
            {
                var intersection = Plane.Intersects(plane, box);
                if (intersection == PlaneIntersectionType.Back) return 0;
                if (intersection == PlaneIntersectionType.Front)
                {
                    totalIn++;
                }
            }
            if (totalIn == 6)
            {
                return 2;
            }
            return 1;
        }
        public bool IsInBounds(BoundingBox b)
        {
            foreach (Plane plane in Frustum)
            {
                PlaneIntersectionType doesIntersect = Plane.Intersects(plane, b);
                switch (doesIntersect)
                {
                    case PlaneIntersectionType.Back:
                        return false;
                    case PlaneIntersectionType.Front:
                        return true;
                }
            }

            return true;
        }

    }
}
