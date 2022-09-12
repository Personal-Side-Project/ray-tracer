using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent ray hit data, including the position and
    /// normal of a hit (and optionally other computed vectors).
    /// </summary>
    public class RayHit
    {
        private Vector3 position;
        private Vector3 normal;
        private Vector3 incident;
        private Vector3 reflection;
        private Vector3 refraction;
        private Vector3 transmission;

        public RayHit(Vector3 position, Vector3 normal, Vector3 incident, Material material)
        {
            this.position = position;
            this.normal = normal;
            this.incident = incident;

            // Compute the reflection
            this.reflection = incident - 2 * incident.Dot(normal) * normal;

            // compute the transmission
            this.transmission = new Vector3();
            this.refraction = new Vector3();
        }

        // You may wish to write methods to compute other vectors, 
        // e.g. reflection, transmission, etc

        public Vector3 Position { get { return this.position; } }

        public Vector3 Normal { get { return this.normal; } }

        public Vector3 Incident { get { return this.incident; } }



        public Vector3 Reflection { get { return this.reflection; } }

        public Vector3 Refraction { get { return this.refraction; } }



        public Vector3 Transmission { get { return this.transmission; } }
    }
}
