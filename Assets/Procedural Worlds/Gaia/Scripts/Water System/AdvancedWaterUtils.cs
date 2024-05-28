using System.Collections.Generic;
using UnityEngine;

namespace Gaia
{
    [System.Serializable]
    public class FrustumData
    {
        public bool m_useFrustumCalculation = false;
        public int m_worldSize = 1024;
        public Vector2 m_worldOrigin = new Vector2(-512f, -512f);
        public int m_cullingGroupResolution = 100;
        public bool m_drawGizmos= false;
        public bool m_debug = false;
        public float m_percentageCount = 5f;
        public CullingGroup m_cullingGroup;
        public int m_resultsCount;
    }
    [System.Serializable]
    public class PositionCheckData
    {
        public bool m_usePositionCheck = true;
        public Vector3 m_lastPosition = Vector3.zero;
        public Vector3 m_lastRotation = Vector3.zero;
    }

    public class AdvancedWaterUtils
    {
        #region Utils

        /// <summary>
        /// Checks to see if the position has changed
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool PositionChanged(Camera camera, ref PositionCheckData data)
        {
            if (data == null)
            {
                return false;
            }
            if (camera == null)
            {
                Debug.LogError("The main camera is null. The position check will always return false if the camera value is null. Please make sure the camera value has been set if you're using 'Use Position Check'");
                return false;
            }

            if (!data.m_usePositionCheck)
            {
                return true;
            }
            else
            {
                Vector3 currentPostion = camera.transform.position;
                Vector3 currentRotation = camera.transform.eulerAngles;
                if (currentPostion != data.m_lastPosition || currentRotation != data.m_lastRotation)
                {
                    data.m_lastPosition = currentPostion;
                    data.m_lastRotation = currentRotation;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Checks to see if the main camera can see the ocean in any of the frustum planes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool FrustumTest(FrustumData data)
        {
            if (data == null)
            {
                return false;
            }

            if (data.m_cullingGroup == null)
            {
                return false;
            }

            int visableCount = 0;
            for (int i = 0; i < data.m_resultsCount; i++)
            {
                if (data.m_cullingGroup.IsVisible(i))
                {
                    visableCount++;
                }
            }
            if (data.m_debug)
            {
                Debug.Log("Active Count " + visableCount);
                Debug.Log("Min Check Value " + (data.m_resultsCount / 100) * data.m_percentageCount);
            }

            if (visableCount >= (data.m_resultsCount / 100) * data.m_percentageCount)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Buildings the culling group for water reflections culling testing
        /// </summary>
        /// <param name="mainCamera"></param>
        /// <param name="yPosition"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int BuildOceanCullingGroup(Camera mainCamera, float yPosition, ref FrustumData data)
        {
            if (data.m_cullingGroup != null)
            {
                data.m_cullingGroup.Dispose();
            }

            if (!data.m_useFrustumCalculation || mainCamera == null)
            {
                return - 1;
            }

            data.m_cullingGroup = new CullingGroup();

            int results = 0;
            List<BoundingSphere> boundingSpheres = new List<BoundingSphere>();
            for (int row = 0; row < data.m_cullingGroupResolution; ++row)
            {
                for (int columns = 0; columns < data.m_cullingGroupResolution; ++columns)
                {
                    Vector3 newPosition = new Vector3
                    {
                        x = ((columns + 1) * data.m_worldSize / data.m_cullingGroupResolution) - data.m_worldSize / data.m_cullingGroupResolution / 2f + data.m_worldOrigin.x,
                        y = yPosition,
                        z = ((row + 1) * data.m_worldSize / data.m_cullingGroupResolution) - data.m_worldSize / data.m_cullingGroupResolution / 2f + data.m_worldOrigin.y
                    };

                    boundingSpheres.Add(new BoundingSphere(newPosition, 1.5f));
                    results++;
                }
            }

            data.m_cullingGroup.SetBoundingSpheres(boundingSpheres.ToArray());
            data.m_cullingGroup.SetBoundingSphereCount(boundingSpheres.Count);
            data.m_cullingGroup.targetCamera = mainCamera;

            return results;
        }
        /// <summary>
        /// Draws the culling group gizmos, can only be called in on draw or selected gizmos
        /// </summary>
        /// <param name="data"></param>
        /// <param name="waterPlane"></param>
        public static void DrawFrustumGizmos(FrustumData data, Renderer waterPlane)
        {
            if (data == null || waterPlane == null || !data.m_drawGizmos)
            {
                return;
            }

            int id = 0;
            for (int row = 0; row < data.m_cullingGroupResolution; ++row)
            {
                for (int columns = 0; columns < data.m_cullingGroupResolution; ++columns)
                {
                    Vector3 newPosition = new Vector3
                    {
                        x = ((columns + 1) * data.m_worldSize / data.m_cullingGroupResolution) - data.m_worldSize / data.m_cullingGroupResolution / 2f + data.m_worldOrigin.x,
                        y = waterPlane.transform.position.y,
                        z = ((row + 1) * data.m_worldSize / data.m_cullingGroupResolution) - data.m_worldSize / data.m_cullingGroupResolution / 2f + data.m_worldOrigin.y
                    };

                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(newPosition, 1.5f);
                    id++;
                }
            }
        }
        /// <summary>
        /// Sets a new percentage value
        /// </summary>
        /// <param name="value"></param>
        public static void SetNewPercentageValue(float value)
        {
            float newValue = Mathf.Clamp(value, 0.1f, 100f);
#if HDPipeline
            GaiaPlanarReflectionsHDRP planarReflectionsHdrp = GaiaPlanarReflectionsHDRP.Instance;
            if (planarReflectionsHdrp != null)
            {
                planarReflectionsHdrp.m_frustumData.m_percentageCount = newValue;
            }
#endif
        }

        #endregion
    }
}