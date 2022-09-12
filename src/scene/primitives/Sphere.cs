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

            const double EPSILON = 1e-6;
            Vector3 D = ray.Direction, O = ray.Origin, oc = O - center;
            double a = D.LengthSq(), b = 2 * (D.Dot(oc)), c = oc.LengthSq() - radius * radius;
            double d = b * b - 4 * a * c;

            if (d < EPSILON)
            {
                return null;
            }
            else
            {
                double t1 = (-b + Math.Sqrt(d)) / 2 * a, t2 = (-b - Math.Sqrt(d)) / 2 * a;
                double t = Math.Min(t1, t2);
                Vector3 P = (O + t * D);
                Vector3 normal = (P - center).Normalized();
                return new RayHit(P, normal, (P - O), material);
            }
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }

    }

}
