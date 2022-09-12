using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            Vector3 D = ray.Direction, O = ray.Origin, P0 = (O - v0);
            Vector3 edge1 = (v1 - v0), edge2 = (v2 - v0);
            Vector3 normal = edge1.Cross(edge2).Normalized(), detVec = D.Cross(edge2), Vvec = P0.Cross(edge1);

            // Boundary intersection tests
            double det = edge1.Dot(detVec);
            if (Math.Abs(det) < Double.Epsilon) return null;

            double u = P0.Dot(detVec) / det, v = Vvec.Dot(D) / det;
            if (u <= 0 | u > 1) return null;
            if (v < 0 | (u + v - 1) >= 0) return null;

            // output the ray
            double t = edge2.Dot(P0.Cross(edge1)) / det;
            if (t < Double.Epsilon) return null;

            Vector3 P = (O + t * D);
            return new RayHit(P, normal, D, material);
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
