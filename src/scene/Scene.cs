using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();

        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        public void Render(Image outputImage)
        {
            /* Initial */
            Ray ray;
            Color pixelColor = new Color(0, 0, 0);
            Vector3 ori = options.CameraPosition;
            int recurseTime = 0;

            /* Options */
            int AA = options.AAMultiplier;// If need Anti-aliasing multiplier
            Vector3 axis = options.CameraAxis;
            double angle = options.CameraAngle;
            double focal = options.FocalLength; // implement Depth of field
            double rad = options.ApertureRadius; // implement Depth of field

            /* Camera's direciton along +z(up) +x(right) +y(left) */
            double FOV = 60;
            double scale = Math.Tan((FOV / 2) * (Math.PI / 180)); // Ensure a horizontal field-of-view of 60's
            bool ratioBool = (outputImage.Width > outputImage.Height);
            double aspectRatio = outputImage.Width / outputImage.Height;

            // Start Rendering pixel by pixel
            for (int x = 0; x < outputImage.Width; x++)
            {
                double locX = (x + 0.5) / outputImage.Width;
                double cameraX = ratioBool ? ((locX * 2 - 1) * scale) : ((locX * 2 - 1) * scale * aspectRatio);

                for (int y = 0; y < outputImage.Height; y++)
                {
                    double locY = (y + 0.5) / outputImage.Height;
                    double cameraY = ratioBool ? ((1 - locY * 2) * scale / aspectRatio) : ((1 - locY * 2) * scale);
                    if (angle > 0)
                    {
                        cameraY *= angle;
                    }

                    if (AA == 1)
                    {
                        Vector3 rayDirection = (new Vector3(cameraX, cameraY, 0) + axis).Normalized();
                        ray = new Ray(ori, rayDirection);
                        pixelColor = Tracer(ray, recurseTime);
                    }
                    else
                    {
                        // Anti-aliasing
                        for (int i = 1; i <= AA; i++)
                        {
                            for (int j = 1; j <= AA; j++)
                            {
                                double xDir = i % 2 == 0 ? 1 : -1;
                                double yDir = j % 2 == 0 ? 1 : -1;
                                double xMove = (i * xDir) / (outputImage.Width * AA * 2);
                                double yMove = (j * yDir) / (outputImage.Height * AA * 2);
                                Vector3 rayDirection = (new Vector3(cameraX + xMove, cameraY + yMove, 0) + axis).Normalized();
                                ray = new Ray(ori, rayDirection);
                                pixelColor += Tracer(ray, recurseTime);
                            }
                        }
                        pixelColor /= (AA * AA);
                    }


                    pixelColor = NormalizedColor(pixelColor);
                    outputImage.SetPixel(x, y, pixelColor);
                }
            }
        }

        private Color Tracer(Ray ray, int recurseTime)
        {
            Ray newRay;
            Vector3 newOri, newDir;
            Color newColor = new Color(0, 0, 0), reflectColor = new Color(0, 0, 0), refractColor = new Color(0, 0, 0);
            double far = Double.PositiveInfinity;
            SceneEntity curE = null;
            RayHit curH = null;
            double reflectRatio = 0;
            // Stage 3 option B
            Boolean ambient = options.AmbientLightingEnabled;

            if (recurseTime == 4) return newColor;
            // Loop all entities and find the closest hit object.
            foreach (SceneEntity entity in this.entities)
            {
                RayHit hit = entity.Intersect(ray);

                if (hit != null)
                {
                    // Choose the front most entity.
                    if ((hit.Position - ray.Origin).LengthSq() < far)
                    {
                        curH = hit;
                        curE = entity;
                        far = (hit.Position - ray.Origin).LengthSq();
                    }
                }
            }

            if (curH == null) return newColor;

            // Offset
            bool outside = curH.Incident.Dot(curH.Normal) < 0;
            Vector3 offset = curH.Normal * 0.0000000001;

            // Diffusion - Ambient Light
            if (ambient && curE.Material.Type == Material.MaterialType.Diffuse)
            {
                Color directLight = new Color(0, 0, 0);
                Color inDirectLight = new Color(0, 0, 0);
                Vector3 L;
                Vector3 N;
                Vector3 Nt;
                Vector3 Nb;

                // Compute direct illumination.
                foreach (PointLight light in lights)
                {
                    if (!shadow(light, curH, curE))
                    {
                        // C = (N dot L) * Cm * Cl
                        N = curH.Normal;
                        L = (light.Position - curH.Position).Normalized();
                        directLight += curE.Material.Color * light.Color * N.Dot(L);
                    }
                }

                // Compute indirect illumination.
                // Compute a rotation matrix.
                var rand = new Random();
                int sampleSize = 8;

                // Create coordinate system.
                N = curH.Normal;

                if (Math.Abs(N.X) > Math.Abs(N.Y))
                {
                    Nt = new Vector3(N.Z, 0, -N.X) / Math.Sqrt(N.X * N.X + N.Z * N.Z);
                }
                else
                {
                    Nt = new Vector3(0, -N.Z, N.Y) / Math.Sqrt(N.Y * N.Y + N.Z * N.Z);
                }
                Nt = Nt.Normalized();
                Nb = N.Cross(Nt).Normalized();

                double pdf = 1 / (2 * Math.PI);

                for (int i = 0; i < sampleSize; i++)
                {
                    double r1 = rand.NextDouble();
                    double r2 = rand.NextDouble();
                    Vector3 sample = uniformSampleHemisphere(r1, r2);
                    Vector3 sampleWorld = new Vector3(
                    sample.X * Nb.X + sample.Y * N.X + sample.Z * Nt.X,
                    sample.X * Nb.Y + sample.Y * N.Y + sample.Z * Nt.Y,
                    sample.X * Nb.Z + sample.Y * N.Z + sample.Z * Nt.Z);

                    newOri = outside ? curH.Position + offset : curH.Position - offset;
                    newDir = sampleWorld.Normalized();
                    newRay = new Ray(newOri, newDir);
                    Color sampleColor = Tracer(newRay, recurseTime + 1) / pdf;
                    inDirectLight += sampleColor;
                }

                inDirectLight /= sampleSize;

                newColor = (directLight + inDirectLight) * curE.Material.Color / Math.PI;

                return newColor;
            }

            switch (curE.Material.Type)
            {
                case RayTracer.Material.MaterialType.Diffuse:
                    foreach (PointLight light in lights)
                    {
                        if (!shadow(light, curH, curE))
                        {
                            // C = (N dot L) * Cm * Cl
                            Vector3 N = curH.Normal;
                            Vector3 L = (light.Position - curH.Position).Normalized();
                            newColor += curE.Material.Color * light.Color * N.Dot(L);
                        }
                    }
                    return newColor;
                case RayTracer.Material.MaterialType.Reflective:
                    newOri = outside ? curH.Position + offset : curH.Position - offset;
                    // R = I − 2 * (N dot I) * N
                    newDir = (curH.Incident - 2 * (curH.Normal.Dot(curH.Incident)) * curH.Normal).Normalized();
                    newRay = new Ray(newOri, newDir);
                    newColor = Tracer(newRay, recurseTime + 1);
                    return newColor;

                case RayTracer.Material.MaterialType.Refractive:
                    // The Fresnel effect with reflective ray.                        
                    // Fresnel ratio
                    Vector3 n = curH.Normal;
                    double cosi = Math.Clamp(curH.Incident.Dot(n), -1, 1);
                    double etai = 1;
                    double etat = curE.Material.RefractiveIndex;

                    // outside the surface
                    if (cosi < 0)
                    {
                        cosi = -cosi;
                    }
                    else
                    {
                        double temp = etai;
                        etai = etat;
                        etat = temp;
                        n = -curH.Normal;
                    }

                    double eta = etai / etat;
                    double k = 1 - eta * eta * (1 - cosi * cosi);
                    double sint = etai / etat * Math.Sqrt(Math.Max(0, 1 - cosi * cosi));

                    if (sint >= 1)
                    {
                        reflectRatio = 1;
                    }
                    else
                    {
                        cosi = Math.Abs(cosi);
                        double cost = Math.Sqrt(Math.Max(0, 1 - sint * sint));
                        double rs = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost));
                        double rp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));
                        reflectRatio = (rs * rs + rp * rp) / 2;
                    }

                    if (reflectRatio < 1)
                    {
                        // Refractive ray.
                        newOri = outside ? curH.Position - offset : curH.Position + offset;
                        newDir = (eta * curH.Incident + (eta * cosi - Math.Sqrt(k)) * n).Normalized();
                        newRay = new Ray(newOri, newDir);
                        refractColor = Tracer(newRay, recurseTime + 1);
                    }

                    // Reflective ray.
                    newOri = outside ? curH.Position + offset : curH.Position - offset;
                    newDir = (curH.Incident - 2 * (curH.Normal.Dot(curH.Incident)) * curH.Normal).Normalized();
                    newRay = new Ray(newOri, newDir);
                    reflectColor = Tracer(newRay, recurseTime + 1);
                    newColor = refractColor * (1 - reflectRatio) + reflectColor * (reflectRatio);
                    return newColor;
                case RayTracer.Material.MaterialType.Emissive:
                    foreach (PointLight light in lights)
                    {
                        if (!shadow(light, curH, curE))
                        {
                            // C = (N dot L) * Cm * Cl
                            Vector3 N = curH.Normal;
                            Vector3 L = (light.Position - curH.Position).Normalized();
                            newColor += curE.Material.Color * N.Dot(L);
                        }
                    }
                    return newColor;
            }


            return newColor;
        }

        private bool shadow(PointLight lit, RayHit frontHit, SceneEntity s)
        {
            Vector3 p = frontHit.Position;
            Ray hit = new Ray(p, (lit.Position - p).Normalized());

            foreach (SceneEntity each in this.entities)
            {
                RayHit curH = each.Intersect(hit);
                if (curH != null)
                {
                    double disL = (lit.Position - frontHit.Position).LengthSq();
                    double disE = (curH.Position - frontHit.Position).LengthSq();
                    if (disL > disE && each != s)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Color NormalizedColor(Color color)
        {
            if (color.R > 1) color = new Color(1, color.G, color.B);
            if (color.G > 1) color = new Color(color.R, 1, color.B);
            if (color.B > 1) color = new Color(color.R, color.G, 1);
            if (color.R < 0) color = new Color(0, color.G, color.B);
            if (color.G < 0) color = new Color(color.R, 0, color.B);
            if (color.B < 0) color = new Color(color.R, color.G, 0);

            return color;
        }

        private Vector3 uniformSampleHemisphere(double r1, double r2)
        {
            double sinTheta = Math.Sqrt(1 - r1 * r1);
            double phi = 2 * Math.PI * r2;
            double x = sinTheta * Math.Cos(phi);
            double z = sinTheta * Math.Sin(phi);
            return new Vector3(x, r1, z);
        }

    }
}
