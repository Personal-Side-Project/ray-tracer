using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // (d.d) * t^2 + 2 * d.(e - c) * t + (e - c).(e - c) - R^2 = 0
            // => a * t^2 + b * t + c = 0

            Vector3 D = ray.Direction, O = ray.Origin, oc = O - center;
            double a = D.LengthSq(), b = 2 * (D.Dot(oc)), c = oc.LengthSq() - radius * radius;
            double d = b * b - 4 * a * c;

            if (d < 0) return null;
            double t2 = (-b - Math.Sqrt(d)) / 2 * a;
            double t;
            if (t2 > 0)
            {
                t = t2;
            }
            else
            {
                t = (-b + Math.Sqrt(d)) / 2 * a;
                if (t < 0) return null;
            }
            Vector3 P = (O + t * D);
            Vector3 normal = (P - center).Normalized();
            return new RayHit(P, normal, D, material);
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }

    }

}
