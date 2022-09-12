using System;
using System.Collections.Generic;
using System.Linq;

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
            const double EPSILON = 1e-10;
            double FOV = 60, angle = options.CameraAngle;
            Vector3 ori = options.CameraPosition, axis = options.CameraAxis;
            // Camera is situated at the origin (0, 0, 0) as default
            // And The camera is looking forward along +z(up) +x(right) +y(left)
            bool ratioBool = (outputImage.Width > outputImage.Height);
            double aspectRatio = outputImage.Width / outputImage.Height;
            double scale = Math.Tan((FOV / 2) * (Math.PI / 180)); // Ensure a horizontal field-of-view of 60's

            // If need Anti-aliasing multiplier
            int antiAliasing = options.AAMultiplier;

            // implement Depth of field
            double rad = options.ApertureRadius;
            double focal = options.FocalLength;

            // Start Rendering pixel by pixel
            for (int x = 0; x <= outputImage.Width - 1; x++)
            {
                double locX = (x + 0.5) / outputImage.Width;
                double cameraX = ratioBool ? ((locX * 2 - 1) * scale) : ((locX * 2 - 1) * scale * aspectRatio);

                for (int y = 0; y <= outputImage.Height - 1; y++)
                {
                    double locY = (y + 0.5) / outputImage.Height;
                    double cameraY = ratioBool ? ((1 - locY * 2) * scale / aspectRatio) : ((1 - locY * 2) * scale);

                    Vector3 rayDirection = (new Vector3(cameraX, cameraY, 0) + axis).Normalized();
                    Ray firing = new Ray(ori, rayDirection);

                    double closest = 0; // check which material is the closest
                    bool gotHit = false;

                    // Handle Lights
                    List<RayHit> orders = new List<RayHit>();
                    List<SceneEntity> ss = new List<SceneEntity>();
                    foreach (SceneEntity ele in this.entities)
                    {
                        // Handle multiple objects
                        RayHit hit = ele.Intersect(firing);
                        Color diffuse = new Color(0, 0, 0);

                        if (hit != null)
                        {
                            double curDistance = hit.Incident.Length();

                            if (!gotHit)
                            {
                                ss.Add(ele);
                                orders.Add(hit);
                                gotHit = true;
                                closest = curDistance;
                            }
                            if (curDistance > closest)
                            {
                                ss.Insert(0, ele);
                                orders.Insert(0, hit);
                                continue;
                            }
                            else
                            {
                                ss.Add(ele);
                                orders.Add(hit);
                                closest = curDistance;
                            }
                        }

                    }

                    Color allC = new Color(0, 0, 0);
                    for (int i = 1; i <= orders.Count - 1; i++)
                    {
                        RayHit obj = orders[i];
                        SceneEntity m = ss[i];
                        allC = new Color(0, 0, 0);
                        foreach (PointLight lit in this.lights)
                        {
                            Color effective_color = m.Material.Color * lit.Color; // know the colour if it gets light hit
                            double NL = (lit.Position - obj.Position).Normalized().Dot(obj.Normal);
                            if (NL < EPSILON) continue;

                            if (!shadow(lit, obj.Position + EPSILON * obj.Normal, m))
                            {
                                allC += draw(m, effective_color * NL);
                            }
                        }
                        outputImage.SetPixel(x, y, allC);
                    }

                }
            }
        }

        public bool shadow(PointLight lit, Vector3 p, SceneEntity s)
        {
            const double EPSILON = 0.0000001;
            Vector3 ray = (lit.Position - p);
            double dis = ray.Length();

            Ray hit = new Ray(p, ray.Normalized());

            bool shadowed = false;
            List<double> check = new List<double>();
            foreach (SceneEntity each in this.entities)
            {
                RayHit curH = each.Intersect(hit);
                if (curH != null)
                {
                    double cur = (lit.Position - curH.Position).Length();
                    if (dis - cur > EPSILON) check.Add(cur);
                }
            }

            if (check.Count > 0)
            {
                for (int i = 0; i <= check.Count - 1; i++)
                {
                    if (check[i] < check.Max())
                    {
                        continue;
                    }
                    else
                    {
                        shadowed = true;
                        break;
                    }
                }
            }
            return shadowed;
        }

        public Color draw(SceneEntity ele, Color diffuse)
        {
            Color result = new Color(0, 0, 0);
            switch (ele.Material.Type)
            {
                case RayTracer.Material.MaterialType.Diffuse:
                    /// Use Lambert's Cosine Law
                    result += diffuse;
                    break;

                case RayTracer.Material.MaterialType.Reflective:
                    break;

                case RayTracer.Material.MaterialType.Refractive:
                    break;
            }

            return result;
        }

    }
}
