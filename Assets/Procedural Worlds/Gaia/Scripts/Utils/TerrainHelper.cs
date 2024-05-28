using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using static Gaia.GaiaConstants;
#if !UNITY_2021_2_OR_NEWER
using UnityEngine.Experimental.TerrainAPI;
#else
using UnityEngine.TerrainTools;
#endif
using UnityEditor;

namespace Gaia
{

    public enum StitchDirection { North, South, West, East };

    /// <summary>
    /// Terrain utility functions
    /// </summary>
    public class TerrainHelper : MonoBehaviour
    {
        [Range(1, 5), Tooltip("Number of smoothing interations to run. Can be run multiple times.")]
        public int m_smoothIterations = 1;



        //Knock ourselves out if we happen to be left there in play mode
        void Awake()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Flatten all the active terrains
        /// </summary>
        public static void Flatten()
        {
            FlattenTerrain(Terrain.activeTerrains);
        }

        /// <summary>
        /// Flatten the terrain passed in
        /// </summary>
        /// <param name="terrain">Terrain to be flattened</param>
        public static void FlattenTerrain(Terrain terrain)
        {
            float[,] heights = new float[terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution];
            terrain.terrainData.SetHeights(0, 0, heights);
        }

        /// <summary>
        /// Flatten all the terrains passed in
        /// </summary>
        /// <param name="terrains">Terrains to be flattened</param>
        public static void FlattenTerrain(Terrain[] terrains)
        {
            foreach (Terrain terrain in terrains)
            {
                float[,] heights = new float[terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution];
                terrain.terrainData.SetHeights(0, 0, heights);
            }
        }

        /// <summary>
        /// Stitch the terrains together with unity set neighbors calls
        /// </summary>
        public static void Stitch()
        {
            StitchTerrains(Terrain.activeTerrains);
        }

        /// <summary>
        /// Stitch the terrains together - wont align them although should update this to support that as well.
        /// </summary>
        /// <param name="terrains">Array of terrains to organise as neighbors</param>
        public static void StitchTerrains(Terrain[] terrains)
        {
            Terrain right = null;
            Terrain left = null;
            Terrain bottom = null;
            Terrain top = null;

            foreach (Terrain terrain in terrains)
            {
                right = null;
                left = null;
                bottom = null;
                top = null;

                foreach (Terrain neighbor in terrains)
                {
                    //Check to see if neighbor is above or below
                    if (neighbor.transform.position.x == terrain.transform.position.x)
                    {
                        if ((neighbor.transform.position.z + neighbor.terrainData.size.z) == terrain.transform.position.z)
                        {
                            top = neighbor;
                        }
                        else if ((terrain.transform.position.z + terrain.terrainData.size.z) == neighbor.transform.position.z)
                        {
                            bottom = neighbor;
                        }
                    }
                    else if (neighbor.transform.position.z == terrain.transform.position.z)
                    {
                        if ((neighbor.transform.position.x + neighbor.terrainData.size.z) == terrain.transform.position.z)
                        {
                            left = neighbor;
                        }
                        else if ((terrain.transform.position.x + terrain.terrainData.size.x) == neighbor.transform.position.x)
                        {
                            right = neighbor;
                        }
                    }
                }

                terrain.SetNeighbors(left, top, right, bottom);
            }
        }

        /// <summary>
        /// Smooth the active terrain - needs to be extended to all and to handle edges
        /// </summary>
        /// <param name="iterations">Number of smoothing iterations</param>
        public void Smooth()
        {
            Smooth(m_smoothIterations);
        }

        /// <summary>
        /// Smooth the active terrain - needs to be extended to all and to handle edges
        /// </summary>
        /// <param name="iterations">Number of smoothing iterations</param>
        public static void Smooth(int iterations)
        {
            UnityHeightMap hm = new UnityHeightMap(Terrain.activeTerrain);
            hm.Smooth(iterations);
            hm.SaveToTerrain(Terrain.activeTerrain);
        }

        /// <summary>
        /// Sets up the given material on all terrains
        /// </summary>
        /// <param name="material">The material to apply</param>
        /// <param name="title">If not empty, this title will be displayed with the other string parameters in a dialog so the user can decide if they want to apply the material or not.</param>
        public static void SetTerrainMaterial(Material material, string title="", string message ="", string OK="", string cancel="")
        {
            //Only perform action if there is any terrain in this scene to begin with
            if (GaiaUtils.HasTerrains())
            {
                if (title == "" || GaiaUtils.DisplayDialogNoEditor("Set up Terrain Material?", message, "Continue", "Cancel"))
                {
                    if (GaiaUtils.HasDynamicLoadedTerrains())
                    {
                        Action<Terrain> act = (terrain) => { terrain.materialTemplate = material; };
                        GaiaUtils.CallFunctionOnDynamicLoadedTerrains(act, true, null, "Applying Material to Terrains...");
                    }
                    else
                    {
                        foreach (Terrain terrain in Terrain.activeTerrains)
                        {
                            terrain.materialTemplate = material;
                        }
                    }
                }
            }
        }




        /// <summary>
        /// Get the vector of the centre of the active terrain, and flush to ground level if asked to
        /// </summary>
        /// <param name="flushToGround">If true set it flush to the ground</param>
        /// <returns>Vector3.zero if no terrain, otherwise the centre of it</returns>
        public static Vector3 GetActiveTerrainCenter(bool flushToGround = true)
        {
            Bounds b = new Bounds();
            Terrain t = GetActiveTerrain();
            if (GetTerrainBounds(t, ref b))
            {
                if (flushToGround == true)
                {
                    return new Vector3(b.center.x, t.SampleHeight(b.center), b.center.z);
                }
                else
                {
                    return b.center;
                }
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Gets the world map terrain from the scene
        /// </summary>
        /// <returns>The world map terrain</returns>
        public static Terrain GetWorldMapTerrain()
        {
            foreach (Terrain t in Terrain.activeTerrains)
            {
                if (TerrainHelper.IsWorldMapTerrain(t))
                {
                    return t;
                }
            }

            //still no world map terrain? might be a deactivated GameObject, check those as well
            GameObject worldMapGO = GaiaUtils.FindObjectDeactivated(GaiaConstants.worldMapTerrainPrefix + "_", false);
            if (worldMapGO != null)
            {
                Terrain t = worldMapGO.GetComponent<Terrain>();
                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>
        /// Get any active terrain - pref active terrain
        /// </summary>
        /// <returns>Any active terrain or null</returns>
        public static Terrain GetActiveTerrain()
        {
            //Grab active terrain if we can
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null && terrain.isActiveAndEnabled)
            {
                return terrain;
            }

            //Then check rest of terrains
            for (int idx = 0; idx < Terrain.activeTerrains.Length; idx++)
            {
                terrain = Terrain.activeTerrains[idx];
                if (terrain != null && terrain.isActiveAndEnabled)
                {
                    return terrain;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the layer mask of the active terrain, or default if there isnt one
        /// </summary>
        /// <returns>Layermask of activer terrain or default if there isnt one</returns>
        public static LayerMask GetActiveTerrainLayer()
        {
            LayerMask layer = new LayerMask();
            Terrain terrain = GetActiveTerrain();
            if (terrain != null)
            {
                layer.value = 1 << terrain.gameObject.layer;
                return layer;
            }
            layer.value = 1 << LayerMask.NameToLayer("Default");
            return layer;
        }

        /// <summary>
        /// Get the layer mask of the active terrain, or default if there isnt one
        /// </summary>
        /// <returns>Layermask of activer terrain or default if there isnt one</returns>
        public static LayerMask GetActiveTerrainLayerAsInt()
        {
            LayerMask layerValue = GetActiveTerrainLayer().value;
            for (int layerIdx = 0; layerIdx < 32; layerIdx++)
            {
                if (layerValue == (1 << layerIdx))
                {
                    return layerIdx;
                }
            }
            return LayerMask.NameToLayer("Default");
        }

        /// <summary>
        /// Get the number of active terrain tiles in this scene
        /// </summary>
        /// <returns>Number of terrains in the scene</returns>
        public static int GetActiveTerrainCount()
        {
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                //with terrain loading, we can simply count the loaded scenes, those should be the active terrains
                return TerrainLoaderManager.TerrainScenes.Where(x => x.m_regularLoadState == LoadState.Loaded).Count() + TerrainLoaderManager.TerrainScenes.Where(x => x.m_impostorLoadState == LoadState.Loaded).Count();

            }
            else
            {
                //For non-terrain loading we need to take a look at what we can find in the scene
                //Regular terrains
                Terrain terrain;
                int terrainCount = 0;
                for (int idx = 0; idx < Terrain.activeTerrains.Length; idx++)
                {
                    terrain = Terrain.activeTerrains[idx];
                    if (terrain != null && terrain.isActiveAndEnabled)
                    {
                        terrainCount++;
                    }
                }

                //Mesh Terrains from a terrain export
                GameObject exportContainer = GaiaUtils.GetTerrainExportObject(false);
                if (exportContainer != null)
                {
                    //Iterate through the objects in here, if it is active and the name checks out we can assume it is a mesh terrain.
                    foreach (Transform t in exportContainer.transform)
                    {
                        if (t.gameObject.GetComponent<MeshRenderer>() != null)
                        {
                            if (t.gameObject.activeInHierarchy && (t.name.StartsWith(GaiaConstants.MeshTerrainName) || t.name.StartsWith(GaiaConstants.MeshTerrainLODGroupPrefix)))
                            {
                                terrainCount++;
                            }
                        }
                    }
                }
                return terrainCount;
            }
        }
        #if GAIA_PRO_PRESENT
        /// <summary>
        /// Get the terrain scene that matches this location, otherwise return null
        /// </summary>
        /// <param name="locationWU">Location to check in world units</param>
        /// <returns>Terrain here or null</returns>
        public static TerrainScene GetDynamicLoadedTerrain(Vector3 locationWU, GaiaSessionManager gsm = null)
        {
            if (gsm == null)
            {
                gsm = GaiaSessionManager.GetSessionManager(false);
            }

            foreach (TerrainScene terrainScene in TerrainLoaderManager.TerrainScenes)
            {
                if (terrainScene.m_bounds.min.x <= locationWU.x && terrainScene.m_bounds.min.z <= locationWU.z && terrainScene.m_bounds.max.x >= locationWU.x && terrainScene.m_bounds.max.z >= locationWU.z)
                {
                    return terrainScene;
                }
            }

            return null;
        }
#endif
        /// <summary>
        /// Returns the Quaternion rotation from the active terrain normal
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="playerObj"></param>
        /// <returns></returns>
        public static Vector3 GetRotationFromTerrainNormal(Terrain terrain, GameObject playerObj)
        {
            if (terrain != null && playerObj != null)
            {
                float scalarX = (playerObj.transform.position.x - terrain.transform.position.x) / (float)terrain.terrainData.size.x;
                float scalarZ = (playerObj.transform.position.z - terrain.transform.position.z) / (float)terrain.terrainData.size.z;
                Vector3 interpolatedNormal = terrain.terrainData.GetInterpolatedNormal(scalarX, scalarZ);
                Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, interpolatedNormal) * playerObj.transform.rotation;
                return quaternion.eulerAngles;
            }
            else
            {
                return Vector3.zero;
            }
        }
        /// <summary>
        /// Get the terrain that matches this location, otherwise return null
        /// </summary>
        /// <param name="locationWU">Location to check in world units</param>
        /// <returns>Terrain here or null</returns>
        public static Terrain GetTerrain(Vector3 locationWU, bool selectWorldMapTerrains = false)
        {
            Terrain terrain;
            Vector3 terrainMin = new Vector3();
            Vector3 terrainMax = new Vector3();

            //First check active terrain - most likely already selected
            terrain = Terrain.activeTerrain;
            if (terrain != null && terrain.terrainData !=null &&(selectWorldMapTerrains == TerrainHelper.IsWorldMapTerrain(terrain)))
            {
                terrainMin = terrain.GetPosition();
                terrainMax = terrainMin + terrain.terrainData.size;
                if (locationWU.x >= terrainMin.x && locationWU.x <= terrainMax.x)
                {
                    if (locationWU.z >= terrainMin.z && locationWU.z <= terrainMax.z)
                    {
                        return terrain;
                    }
                }
            }

            //Then check rest of terrains
            Terrain closestTerrain = null;
            float closestDistance = float.MaxValue;
            for (int idx = 0; idx < Terrain.activeTerrains.Length; idx++)
            {
                terrain = Terrain.activeTerrains[idx];
                if (terrain.terrainData==null || (selectWorldMapTerrains != TerrainHelper.IsWorldMapTerrain(terrain)))
                {
                    continue;
                }
                terrainMin = terrain.GetPosition();
                terrainMax = terrainMin + terrain.terrainData.size;

                if (locationWU.x >= terrainMin.x && locationWU.x <= terrainMax.x)
                {
                    if (locationWU.z >= terrainMin.z && locationWU.z <= terrainMax.z)
                    {
                        return terrain;
                    }
                }

                if (closestTerrain == null || Vector3.Distance(terrain.transform.position, locationWU) < closestDistance)
                {
                    closestTerrain = terrain;
                }
            }
            return closestTerrain;
        }

        /// <summary>
        /// Get the bounds of the space encapsulated by the supplied terrain
        /// </summary>
        /// <param name="terrain">Terrain to get bounds for</param>
        /// <param name="bounds">Bounds to update</param>
        /// <returns>True if we got some terrain bounds</returns>
        public static bool GetTerrainBounds(Terrain terrain, ref Bounds bounds)
        {
            if (terrain == null)
            {
                return false;
            }
            bounds.center = terrain.transform.position;
            bounds.size = terrain.terrainData.size;
            bounds.center += bounds.extents;
            return true;
        }


        /// <summary>
        /// Get the bounds of the terrain at this location or fail with a null
        /// </summary>
        /// <param name="locationWU">Location to check and get terrain for</param>
        /// <returns>Bounds of selected terrain or null if invalid for some reason</returns>
        public static bool GetTerrainBounds(ref BoundsDouble bounds, bool activeTerrainsOnly = false)
        {
            //Terrain terrain = GetTerrain(locationWU);
            //if (terrain == null)
            //{
            //    return false;
            //}
            //bounds.center = terrain.transform.position;
            //bounds.size = terrain.terrainData.size;
            //bounds.center += bounds.extents;

            Vector3Double accumulatedCenter = new Vector3Double();

            //Do we use dynamic loaded terrains in the scene?
            if (GaiaUtils.HasDynamicLoadedTerrains() && !activeTerrainsOnly)
            {
#if GAIA_PRO_PRESENT
                //we do have dynamic terrains -> calculate the bounds according to the terrain scene data in the session
                GaiaSessionManager gsm = GaiaSessionManager.GetSessionManager(false);

                foreach (TerrainScene t in TerrainLoaderManager.TerrainScenes)
                {
                    accumulatedCenter += t.m_bounds.center;
                }

                bounds.center = accumulatedCenter / TerrainLoaderManager.TerrainScenes.Count;

                foreach (TerrainScene t in TerrainLoaderManager.TerrainScenes)
                {
                    bounds.Encapsulate(t.m_bounds);
                }
#endif
            }
            else
            {
                //no placeholder -> calculate bounds according to the active terrains in the scene
                if (Terrain.activeTerrains.Length > 0)
                {
                    foreach (Terrain t in Terrain.activeTerrains)
                    {
                        if (!TerrainHelper.IsWorldMapTerrain(t))
                        {
                            if (t.terrainData != null)
                            {
                                accumulatedCenter += new Vector3Double(t.transform.position) + new Vector3Double(t.terrainData.bounds.extents);
                            }
                            else
                            {
                                Debug.LogWarning("Terrain " + t.name + " in the scene is missing the terrain data object!");
                            }
                        }
                    }
                    bounds.center = accumulatedCenter / Terrain.activeTerrains.Length;
             
                    foreach (Terrain t in Terrain.activeTerrains)
                    {
                        if (!TerrainHelper.IsWorldMapTerrain(t))
                        {
                            if (t.terrainData != null)
                            {
                                Bounds newBounds = new Bounds();
                                newBounds.center = t.transform.position;
                                newBounds.size = t.terrainData.size;
                                newBounds.center += t.terrainData.bounds.extents;
                                bounds.Encapsulate(newBounds);
                            }
                        }
                    }
                }
                else
                {
                    bounds = new BoundsDouble(Vector3Double.zero, Vector3Double.zero);
                    //No active terrains? There might be mesh terrains we can use then
                    GameObject meshTerrainExportObject = GaiaUtils.GetTerrainExportObject(false);
                    if (meshTerrainExportObject != null)
                    {
                        foreach (Transform t in meshTerrainExportObject.transform)
                        {
                            MeshRenderer mr = t.GetComponent<MeshRenderer>();
                            if (mr != null)
                            {
                                bounds.Encapsulate(mr.bounds);
                            }
                        }
                    }
                }
            }


            return true;
        }

        /// <summary>
        /// Get a random location on the terrain supplied
        /// </summary>
        /// <param name="terrain">Terrain to check</param>
        /// <param name="start">Start locaton</param>
        /// <param name="radius">Radius to hunt in</param>
        /// <returns></returns>
        public static Vector3 GetRandomPositionOnTerrain(Terrain terrain, Vector3 start, float radius)
        {
            Vector3 newLocation;
            Vector3 terrainMin = terrain.GetPosition();
            Vector3 terrainMax = terrainMin + terrain.terrainData.size;
            while (true)
            {
                //Get a new location
                newLocation = UnityEngine.Random.insideUnitSphere * radius;
                newLocation = start + newLocation;
                //Make sure the new location is within the terrain bounds
                if (newLocation.x >= terrainMin.x && newLocation.x <= terrainMax.x)
                {
                    if (newLocation.z >= terrainMin.z && newLocation.z <= terrainMax.z)
                    {
                        //Update it to be on the terrain surface
                        newLocation.y = terrain.SampleHeight(newLocation);
                        return newLocation;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the bounds of a terrain in world space (The bounds in the terrainData object is in local space of the terrain itself)
        /// </summary>
        /// <param name="t">The terrain to get the bounds in world space for.</param>
        /// <returns></returns>
        public static Bounds GetWorldSpaceBounds(Terrain t)
        {
            Bounds worldSpaceBounds = t.terrainData.bounds;
            worldSpaceBounds.center = new Vector3(worldSpaceBounds.center.x + t.transform.position.x, worldSpaceBounds.center.y + t.transform.position.y, worldSpaceBounds.center.z + t.transform.position.z);
            return worldSpaceBounds;
        }

        /// <summary>
        /// Resizes the terrain splatmap to a new resolution.
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="targetResolution"></param>
        public static void ResizeSplatmaps(Terrain terrain, int targetResolution)
        {
            TerrainData terrainData = terrain.terrainData;
            Material blitMaterial = TerrainPaintUtility.GetCopyTerrainLayerMaterial();      // special blit that forces copy from highest mip only
            RenderTexture[] resizedSplatMaps = new RenderTexture[terrainData.alphamapTextureCount];

            int targetResolutionU = targetResolution;
            int targetResolutionV = targetResolution;
            float invTargetResolutionU = 1.0f / targetResolutionU;
            float invTargetResolutiuonV = 1.0f / targetResolutionV;

            RenderTexture currentRT = RenderTexture.active;

            for (int i = 0; i < terrainData.alphamapTextureCount; i++)
            {
                Texture2D oldSplatmap = terrainData.alphamapTextures[i];

                int sourceResolutionU = oldSplatmap.width;
                int sourceResolutionV = oldSplatmap.height;
                float invSourceResoultionU = 1.0f / sourceResolutionU;
                float invSourceResolutionV = 1.0f / sourceResolutionV;

                resizedSplatMaps[i] = RenderTexture.GetTemporary(targetResolution, targetResolution, 0, oldSplatmap.graphicsFormat);

                float scaleU = (1.0f - invSourceResoultionU) / (1.0f - invTargetResolutionU);
                float scaleV = (1.0f - invSourceResolutionV) / (1.0f - invTargetResolutiuonV);
                float offsetU = 0.5f * (invSourceResoultionU - scaleU * invTargetResolutionU);
                float offsetV = 0.5f * (invSourceResolutionV - scaleV * invTargetResolutiuonV);

                Vector2 scale = new Vector2(scaleU, scaleV);
                Vector2 offset = new Vector2(offsetU, offsetV);

                blitMaterial.mainTexture = oldSplatmap;
                blitMaterial.mainTextureScale = scale;
                blitMaterial.mainTextureOffset = offset;

                oldSplatmap.filterMode = FilterMode.Bilinear;
                RenderTexture.active = resizedSplatMaps[i];
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, targetResolution, 0, targetResolution);
                blitMaterial.SetPass(2);

                RectInt targetPixelRect = new RectInt(0, 0, targetResolution, targetResolution);
                RectInt sourcePixelRect = new RectInt(0, 0, sourceResolutionU, sourceResolutionV);

                if ((targetPixelRect.width > 0) && (targetPixelRect.height > 0))
                {
                    Rect sourceUVs = new Rect(
                        (sourcePixelRect.x) / (float)oldSplatmap.width,
                        (sourcePixelRect.y) / (float)oldSplatmap.height,
                        (sourcePixelRect.width) / (float)oldSplatmap.width,
                        (sourcePixelRect.height) / (float)oldSplatmap.height);

                    Rect sourceUVs2 = new Rect(
                        (sourcePixelRect.x) / (float)oldSplatmap.width,
                        (sourcePixelRect.y) / (float)oldSplatmap.height,
                        (sourcePixelRect.width) / (float)oldSplatmap.width,
                        (sourcePixelRect.height) / (float)oldSplatmap.height);

                    GL.Begin(GL.QUADS);
                    GL.Color(new Color(1.0f, 1.0f, 1.0f, 1.0f));
                    GL.MultiTexCoord2(0, sourceUVs.x, sourceUVs.y);
                    GL.MultiTexCoord2(1, sourceUVs2.x, sourceUVs2.y);
                    GL.Vertex3(targetPixelRect.x, targetPixelRect.y, 0.0f);
                    GL.MultiTexCoord2(0, sourceUVs.x, sourceUVs.yMax);
                    GL.MultiTexCoord2(1, sourceUVs2.x, sourceUVs2.yMax);
                    GL.Vertex3(targetPixelRect.x, targetPixelRect.yMax, 0.0f);
                    GL.MultiTexCoord2(0, sourceUVs.xMax, sourceUVs.yMax);
                    GL.MultiTexCoord2(1, sourceUVs2.xMax, sourceUVs2.yMax);
                    GL.Vertex3(targetPixelRect.xMax, targetPixelRect.yMax, 0.0f);
                    GL.MultiTexCoord2(0, sourceUVs.xMax, sourceUVs.y);
                    GL.MultiTexCoord2(1, sourceUVs2.xMax, sourceUVs2.y);
                    GL.Vertex3(targetPixelRect.xMax, targetPixelRect.y, 0.0f);
                    GL.End();
                }

                GL.PopMatrix();
            }

            terrainData.alphamapResolution = targetResolution;
            for (int i = 0; i < resizedSplatMaps.Length; i++)
            {
                RenderTexture.active = resizedSplatMaps[i];
                terrainData.CopyActiveRenderTextureToTexture(TerrainData.AlphamapTextureName, i, new RectInt(0, 0, targetResolution, targetResolution), Vector2Int.zero, false);
            }
            terrainData.SetBaseMapDirty();
            RenderTexture.active = currentRT;
            for (int i = 0; i < resizedSplatMaps.Length; i++)
                RenderTexture.ReleaseTemporary(resizedSplatMaps[i]);
        }

        /// <summary>
        /// Resizes the heightmap of a terrain to the target resolution
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="targetResolution"></param>
        public static void ResizeHeightmap(Terrain terrain, int targetResolution)
        {
            RenderTexture currentRT = RenderTexture.active;

            RenderTexture oldHeightmap = RenderTexture.GetTemporary(terrain.terrainData.heightmapTexture.descriptor);
            Graphics.Blit(terrain.terrainData.heightmapTexture, oldHeightmap);

            RenderTexture oldHoles = RenderTexture.GetTemporary(terrain.terrainData.holesTexture.width, terrain.terrainData.holesTexture.height, 0, Terrain.holesRenderTextureFormat);
            Graphics.Blit(terrain.terrainData.holesTexture, oldHoles);

            int dWidth = terrain.terrainData.heightmapResolution;
            int sWidth = targetResolution;

            Vector3 oldSize = terrain.terrainData.size;
            terrain.terrainData.heightmapResolution = targetResolution;
            terrain.terrainData.size = oldSize;

            oldHeightmap.filterMode = FilterMode.Bilinear;

            float k = (dWidth - 1.0f) / (sWidth - 1.0f) / dWidth;
            float scaleX = (sWidth * k);
            float offsetX = (float)(0.5 / dWidth - 0.5 * k);
            Vector2 scale = new Vector2(scaleX, scaleX);
            Vector2 offset = new Vector2(offsetX, offsetX);

            Graphics.Blit(oldHeightmap, terrain.terrainData.heightmapTexture, scale, offset);
            RenderTexture.ReleaseTemporary(oldHeightmap);

            oldHoles.filterMode = FilterMode.Point;
            RenderTexture newHoles = RenderTexture.GetTemporary(terrain.terrainData.holesTexture.width, terrain.terrainData.holesTexture.height, 0, Terrain.holesRenderTextureFormat);
            Graphics.Blit(oldHoles, newHoles);
            Graphics.CopyTexture(newHoles, terrain.terrainData.holesTexture);
            RenderTexture.ReleaseTemporary(oldHoles);
            RenderTexture.ReleaseTemporary(newHoles);

            RenderTexture.active = currentRT;

            terrain.terrainData.DirtyHeightmapRegion(new RectInt(0, 0, terrain.terrainData.heightmapTexture.width, terrain.terrainData.heightmapTexture.height), TerrainHeightmapSyncControl.HeightAndLod);
            terrain.terrainData.DirtyTextureRegion(TerrainData.HolesTextureName, new RectInt(0, 0, terrain.terrainData.holesTexture.width, terrain.terrainData.holesTexture.height), false);

        }

        /// <summary>
        /// Averages the heightmaps between two unity terrains so that they align at the seam.
        /// </summary>
        /// <param name="terrain1">The first terrain.</param>
        /// <param name="terrain2">The second terrain.</param>
        /// <param name="extraSeamSize">Extra seam in heightmap pixels where the heights are averaged out to better disguise the seam.</param>
        /// <param name="maxDifference">A maximum difference in height for which align pixels to - if no pixes are within the tolerance, no action will be performed at all. This can be used to ensure the terrains somewhat match up before stitching them</param>
        public static void StitchTerrainHeightmaps(Terrain terrain1, Terrain terrain2, int extraSeamSize = 1, float maxDifference = 1f)
        {
            StitchDirection stitchDirection = StitchDirection.North;

            //What is larger, the difference on x or z axis?
            if (Mathf.Abs(terrain1.transform.position.x - terrain2.transform.position.x) > Mathf.Abs(terrain1.transform.position.z - terrain2.transform.position.z))
            {
                if (terrain1.transform.position.x > terrain2.transform.position.x)
                {
                    stitchDirection = StitchDirection.West;
                }
                else
                {
                    stitchDirection = StitchDirection.East;
                }
            }
            else
            {
                if (terrain1.transform.position.z > terrain2.transform.position.z)
                {
                    stitchDirection = StitchDirection.South;
                }
                else
                {
                    stitchDirection = StitchDirection.North;
                }
            }


            int terrain1XBase = 0, terrain1YBase = 0;
            int terrain2XBase = 0, terrain2YBase = 0;
            int seamWidth = 0, seamHeight = 0;
            int seamDiameter = extraSeamSize + 1;

            float[,] terrain1Heights = new float[0, 0];
            float[,] terrain2Heights = new float[0, 0]; 

            switch (stitchDirection)
            {
                case StitchDirection.North:
                    terrain1XBase = Mathf.RoundToInt(Mathf.Max(0, terrain2.transform.position.x - terrain1.transform.position.x) / terrain1.terrainData.heightmapScale.x);
                    terrain1YBase = terrain1.terrainData.heightmapResolution - 1 - extraSeamSize;
                    seamWidth = terrain1.terrainData.heightmapResolution - Mathf.RoundToInt(Mathf.Abs(terrain1.transform.position.x - terrain2.transform.position.x) / terrain1.terrainData.heightmapScale.x);
                    seamHeight = seamDiameter;
                    terrain1Heights = terrain1.terrainData.GetHeights(terrain1XBase, terrain1YBase, seamWidth, seamHeight);

                    terrain2XBase = Mathf.RoundToInt(Mathf.Max(0, terrain1.transform.position.x - terrain2.transform.position.x) / terrain2.terrainData.heightmapScale.x);
                    terrain2YBase = 0;
                    terrain2Heights = terrain2.terrainData.GetHeights(terrain2XBase, terrain2YBase, seamWidth, seamHeight);

                    StitchBordersWithSeam(stitchDirection, seamWidth, extraSeamSize, ref terrain1Heights, ref terrain2Heights);
                    break;
                case StitchDirection.South:
                    terrain1XBase = Mathf.RoundToInt(Mathf.Max(0, terrain1.transform.position.x - terrain2.transform.position.x) / terrain2.terrainData.heightmapScale.x);
                    terrain1YBase = 0;
                    seamWidth = terrain1.terrainData.heightmapResolution - Mathf.RoundToInt(Mathf.Abs(terrain1.transform.position.x - terrain2.transform.position.x) / terrain1.terrainData.heightmapScale.x);
                    seamHeight = seamDiameter;
                    terrain1Heights = terrain1.terrainData.GetHeights(terrain1XBase, terrain1YBase, seamWidth, seamHeight);
                    
                    terrain2XBase = Mathf.RoundToInt(Mathf.Max(0, terrain2.transform.position.x - terrain1.transform.position.x) / terrain1.terrainData.heightmapScale.x);
                    terrain2YBase = terrain1.terrainData.heightmapResolution - 1 - extraSeamSize;
                    terrain2Heights = terrain2.terrainData.GetHeights(terrain2XBase, terrain2YBase, seamWidth, seamDiameter);
                    StitchBordersWithSeam(stitchDirection, seamWidth, extraSeamSize, ref terrain2Heights, ref terrain1Heights);
                    break;
                case StitchDirection.West:
                    terrain1XBase = 0;
                    terrain1YBase = Mathf.RoundToInt(Mathf.Max(0, terrain2.transform.position.z - terrain1.transform.position.z) / terrain1.terrainData.heightmapScale.z);
                    seamWidth = seamDiameter;
                    seamHeight = terrain1.terrainData.heightmapResolution - Mathf.RoundToInt(Mathf.Abs(terrain1.transform.position.z - terrain2.transform.position.z) / terrain1.terrainData.heightmapScale.z);
                    terrain1Heights = terrain1.terrainData.GetHeights(terrain1XBase, terrain1YBase, seamWidth, seamHeight);

                    terrain2XBase = terrain2.terrainData.heightmapResolution - 1 - extraSeamSize; 
                    terrain2YBase = Mathf.RoundToInt(Mathf.Max(0, terrain1.transform.position.z - terrain2.transform.position.z) / terrain2.terrainData.heightmapScale.z);
                    terrain2Heights = terrain2.terrainData.GetHeights(terrain2XBase, terrain2YBase, seamWidth, seamHeight);
                    StitchBordersWithSeam(stitchDirection, seamHeight, extraSeamSize, ref terrain2Heights, ref terrain1Heights);
                    break;
                case StitchDirection.East:
                    terrain1XBase = terrain2.terrainData.heightmapResolution - 1 - extraSeamSize;
                    terrain1YBase = Mathf.RoundToInt(Mathf.Max(0, terrain1.transform.position.z - terrain2.transform.position.z) / terrain2.terrainData.heightmapScale.z);
                    seamWidth = seamDiameter;
                    seamHeight = terrain1.terrainData.heightmapResolution - Mathf.RoundToInt(Mathf.Abs(terrain1.transform.position.z - terrain2.transform.position.z) / terrain1.terrainData.heightmapScale.z);
                    terrain1Heights = terrain1.terrainData.GetHeights(terrain1XBase, terrain1YBase, seamWidth, seamHeight);

                    terrain2XBase = 0;
                    terrain2YBase = Mathf.RoundToInt(Mathf.Max(0, terrain2.transform.position.z - terrain1.transform.position.z) / terrain1.terrainData.heightmapScale.z);
                    terrain2Heights = terrain2.terrainData.GetHeights(terrain2XBase, terrain2YBase, seamWidth, seamHeight);

                    StitchBordersWithSeam(stitchDirection, seamHeight, extraSeamSize, ref terrain1Heights, ref terrain2Heights);
                    break;
            }
            terrain1.terrainData.SetHeights(terrain1XBase, terrain1YBase, terrain1Heights);
            terrain2.terrainData.SetHeights(terrain2XBase, terrain2YBase, terrain2Heights);
            
            //bool wasStitched = AverageHeightPixels(terrain1, terrain2, terrain1XBase, terrain1YBase, terrain2XBase, terrain2YBase, seamWidth, seamHeight, 1, 1, true, true, maxDifference);

            //if (extraSeamSize > 0 && wasStitched)
            //{
            //    for (int s = 1; s <= extraSeamSize; s++)
            //    {
            //        float terrain1Weight = Mathf.InverseLerp(0, extraSeamSize, s);
            //        float terrain2Weight = Mathf.InverseLerp(extraSeamSize, 0, s);

            //        switch (stitchDirection)
            //        {
            //            case StitchDirection.North:
            //                AverageHeightPixels(terrain1, terrain1, terrain1XBase, terrain1YBase - s, terrain1XBase, terrain1YBase, seamWidth, seamHeight, terrain1Weight, terrain2Weight, true, false, 1);
            //                AverageHeightPixels(terrain2, terrain2, terrain2XBase, terrain2YBase + s, terrain2XBase, terrain2YBase, seamWidth, seamHeight, terrain1Weight, terrain2Weight, true, false, 1);
            //                break;
            //            case StitchDirection.South:
            //                AverageHeightPixels(terrain1, terrain1, terrain1XBase, terrain1YBase + s, terrain1XBase, terrain1YBase, seamWidth, seamHeight, terrain1Weight, terrain2Weight, true, false, 1);
            //                AverageHeightPixels(terrain2, terrain2, terrain2XBase, terrain2YBase - s, terrain2XBase, terrain2YBase, seamWidth, seamHeight, terrain1Weight, terrain2Weight, true, false, 1);
            //                break;
            //            case StitchDirection.West:
            //                AverageHeightPixels(terrain1, terrain1, terrain1XBase + s, terrain1YBase, terrain1XBase, terrain1YBase, seamWidth, seamHeight, terrain1Weight, terrain2Weight, true, false, 1);
            //                AverageHeightPixels(terrain2, terrain2, terrain2XBase - s, terrain2YBase, terrain2XBase, terrain2YBase, seamWidth, seamHeight, terrain1Weight, terrain2Weight, true, false, 1);
            //                break;
            //            case StitchDirection.East:
            //                AverageHeightPixels(terrain1, terrain1, terrain1XBase - s, terrain1YBase, terrain1XBase, terrain1YBase, seamWidth, seamHeight, terrain1Weight, terrain2Weight, true, false, 1);
            //                AverageHeightPixels(terrain2, terrain2, terrain2XBase + s, terrain2YBase, terrain2XBase, terrain2YBase, seamWidth, seamHeight, terrain1Weight, terrain2Weight, true, false, 1);
            //                break;
            //        }

            //    }

            //}
        }

        private static void StitchBordersWithSeam(StitchDirection stitchDirection, int seamLength, int extraSeamSize, ref float[,] terrain1Heights, ref float[,] terrain2Heights)
        {
            for (int dimension1 = 0; dimension1 < seamLength; dimension1++)
            {
                float terrain1EndHeight = 0f;
                float terrain2EndHeight = 0f;

                bool isHorizontalSeam = stitchDirection == StitchDirection.North || stitchDirection == StitchDirection.South;

                if (isHorizontalSeam)
                {
                    terrain1EndHeight = terrain1Heights[0, dimension1];
                    terrain2EndHeight = terrain2Heights[extraSeamSize, dimension1];
                }
                else
                {
                    terrain1EndHeight = terrain1Heights[dimension1, 0];
                    terrain2EndHeight = terrain2Heights[dimension1, extraSeamSize];
                }

                //float[] oldPoints = terrain1Heights[dimension1,];

                //if (terrain1EndHeight > 0 || terrain2EndHeight > 0)
                //{
                //    string message = "BEFORE:";
                //    message = "\r\nTerrain 1 End: " + terrain1EndHeight.ToString();
                //    message += "\r\nTerrain 2 End: " + terrain2EndHeight.ToString();
                //    message += "\r\n";
                //    for (int dimension2 = 1; dimension2 < extraSeamSize * 2; dimension2++)
                //    {
                //        if (dimension2 <= extraSeamSize)
                //        {
                //            message += "\r\n" + dimension2.ToString() + ": " + terrain1Heights[dimension2, dimension1].ToString();
                //        }
                //        else
                //        {
                //            message += "\r\n" + dimension2.ToString() + ": " + terrain2Heights[dimension2 - extraSeamSize, dimension1].ToString();
                //        }
                //    }
                //    Debug.Log(message);
                //}

                for (int dimension2 = 1; dimension2 < extraSeamSize * 2; dimension2++)
                {
                    float linearHeight = Mathf.Lerp(terrain1EndHeight, terrain2EndHeight, Mathf.InverseLerp(1, extraSeamSize * 2 - 1, dimension2));

                    if (dimension2 == extraSeamSize)
                    {
                        //Do not process the actual seam between the two terrains, this will be done after the rest of the points have been processed
                    }
                    else
                    {
                        if (dimension2 < extraSeamSize)
                        {
                            if (isHorizontalSeam)
                            {
                                terrain1Heights[dimension2, dimension1] = Mathf.Lerp(terrain1Heights[dimension2, dimension1], linearHeight, Mathf.InverseLerp(0, extraSeamSize, dimension2));
                            }
                            else
                            {
                                terrain1Heights[dimension1, dimension2] = Mathf.Lerp(terrain1Heights[dimension1, dimension2], linearHeight, Mathf.InverseLerp(0, extraSeamSize, dimension2));
                            }
                        }
                        else
                        {
                            if (isHorizontalSeam)
                            {
                                terrain2Heights[dimension2 - extraSeamSize, dimension1] = Mathf.Lerp(linearHeight, terrain2Heights[dimension2 - extraSeamSize, dimension1], Mathf.InverseLerp(0, extraSeamSize, dimension2 - extraSeamSize));
                            }
                            else
                            {
                                terrain2Heights[dimension1, dimension2-extraSeamSize] = Mathf.Lerp(linearHeight, terrain2Heights[dimension1, dimension2 - extraSeamSize], Mathf.InverseLerp(0, extraSeamSize, dimension2 - extraSeamSize));
                            }
                        }
                    }
                }
                //Now do the actual seam between the two points based on the changed data
                //just do the average between the closest points at the actual seam - this gives the best results without any visible bends between two terrains
                if (isHorizontalSeam)
                {
                    terrain1Heights[extraSeamSize, dimension1] = (terrain1Heights[extraSeamSize - 1, dimension1] + terrain2Heights[extraSeamSize + 1 - extraSeamSize, dimension1]) / 2f;
                    terrain2Heights[0, dimension1] = terrain1Heights[extraSeamSize, dimension1];
                }
                else
                {
                    terrain1Heights[dimension1, extraSeamSize] = (terrain1Heights[dimension1, extraSeamSize - 1] + terrain2Heights[dimension1, extraSeamSize + 1 - extraSeamSize]) / 2f;
                    terrain2Heights[dimension1, 0] = terrain1Heights[dimension1, extraSeamSize];
                }

                //if (terrain1EndHeight > 0 || terrain2EndHeight > 0)
                //{
                //    string message = "AFTER:";
                //    message = "\r\nTerrain 1 End: " + terrain1EndHeight.ToString();
                //    message += "\r\nTerrain 2 End: " + terrain2EndHeight.ToString();
                //    message += "\r\n";
                //    for (int dimension2 = 1; dimension2 < extraSeamSize * 2; dimension2++)
                //    {
                //        if (dimension2 <= extraSeamSize)
                //        {
                //            message += "\r\n" + dimension2.ToString() + ": " + terrain1Heights[dimension2, dimension1].ToString();
                //        }
                //        else
                //        {
                //            message += "\r\n" + dimension2.ToString() + ": " + terrain2Heights[dimension2 - extraSeamSize, dimension1].ToString();
                //        }
                //    }
                //    Debug.Log(message);
                //    Debug.Log("################################################");
                //}
            }
        }




        /// <summary>
        /// Tries to find a neighbor terrain in the given direction, and will stitch it if found
        /// </summary>
        /// <param name="direction">The directon in which to look for a neighboring terrain.</param>
        /// <param name="terrain">The terrain where the stitching operation originates.</param>
        /// <param name="extraSeamSize">Amount of extra heightmap pixels to align near the border. More pixels result in smoother transition, but may also remove details on the individual terrains due to the averaging.</param>
        /// <param name="maxDifference">The maximum amount of (scalar) height difference the stitching process will attempt to stitch up. If =1, the stitching process will fix gaps of any size between two terrains.</param>
        /// <returns>True if stitching performed, false if not</returns>
        public static bool TryStitch(Terrain terrain, StitchDirection direction, int extraSeamSize = 1, float maxDifference = 0.01f)
        {
            if (TerrainLoaderManager.Instance.m_autoTerrainStitching == false)
            {
                return false;
            }

            bool stitched = false;
            Terrain neighbor = GetTerrainNeighbor(terrain, direction);
            if (neighbor != null)
            {
                StitchTerrainHeightmaps(terrain, neighbor, extraSeamSize, maxDifference);
                stitched = true;
            }
            else
            {
                stitched = false;
            }

            //store the stitiching result for loaded terrains
            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                TerrainScene ts = TerrainLoaderManager.Instance.GetTerrainSceneAtPosition(terrain.transform.position + terrain.terrainData.size * 0.5f);
                if (ts != null)
                {
                    switch (direction)
                    {
                        case StitchDirection.North:
                            ts.m_stitchedNorthBorder = stitched;
                            break;
                        case StitchDirection.South:
                            ts.m_stitchedSouthBorder = stitched;
                            break;
                        case StitchDirection.West:
                            ts.m_stitchedWestBorder = stitched;
                            break;
                        case StitchDirection.East:
                            ts.m_stitchedEastBorder = stitched;
                            break;
                    }

                    //Log the stitch state for the opposite border for the neighbor, otherwise the neighbor would try to re-stitch the terrain when being reloaded
                    TerrainScene tsN = TerrainLoaderManager.Instance.TryGetNeighbor(ts, direction);
                    if (tsN != null)
                    {
                        switch (direction)
                        {
                            ///OPPOSITE BORDERS!
                            case StitchDirection.North:
                                tsN.m_stitchedSouthBorder = stitched;
                                break;
                            case StitchDirection.South:
                                tsN.m_stitchedNorthBorder = stitched;
                                break;
                            case StitchDirection.West:
                                tsN.m_stitchedEastBorder = stitched;
                                break;
                            case StitchDirection.East:
                                tsN.m_stitchedWestBorder = stitched;
                                break;
                        }
                    }
                }
                if (!Application.isPlaying)
                {
                    TerrainLoaderManager.Instance.DirtyStorageData();
                }
               
            }
            return stitched;
        }

        public static Terrain GetTerrainNeighbor(Terrain terrain, StitchDirection direction)
        {
            Bounds boundsOriginTerrain = GetWorldSpaceBounds(terrain);
            Terrain neighbor = null;
            foreach (Terrain t in Terrain.activeTerrains)
            {
                Bounds boundsTerrain = GetWorldSpaceBounds(t);
                if (boundsTerrain.Intersects(boundsOriginTerrain))
                {
                    switch (direction)
                    {
                        case StitchDirection.North:
                            if (boundsOriginTerrain.max.z == boundsTerrain.min.z && boundsTerrain.max.x > boundsOriginTerrain.min.x && boundsTerrain.min.x < boundsOriginTerrain.max.x)
                            {
                                neighbor = t;
                            }
                            break;
                        case StitchDirection.South:
                            if (boundsOriginTerrain.min.z == boundsTerrain.max.z && boundsTerrain.max.x > boundsOriginTerrain.min.x && boundsTerrain.min.x < boundsOriginTerrain.max.x)
                            {
                                neighbor = t;
                            }
                            break;
                        case StitchDirection.East:
                            if (boundsOriginTerrain.max.x == boundsTerrain.min.x && boundsTerrain.max.z > boundsOriginTerrain.min.z && boundsTerrain.min.z < boundsOriginTerrain.max.z)
                            {
                                neighbor = t;
                            }
                            break;
                        case StitchDirection.West:
                            if (boundsOriginTerrain.min.x == boundsTerrain.max.x && boundsTerrain.max.z > boundsOriginTerrain.min.z && boundsTerrain.min.z < boundsOriginTerrain.max.z)
                            {
                                neighbor = t;
                            }
                            break;
                    }
                    if (neighbor != null)
                    { break; }
                }
            }
            return neighbor;
        }

        private static bool AverageHeightPixels(Terrain terrain1, Terrain terrain2, int terrain1XBase, int terrain1YBase, int terrain2XBase, int terrain2YBase, int seamWidth, int seamHeight, float terrain1Weight, float terrain2Weight, bool applyTo1, bool applyTo2, float maxDifference)
        {
            float[,] terrain1Heights = terrain1.terrainData.GetHeights(terrain1XBase, terrain1YBase, seamWidth, seamHeight);
            float[,] terrain2Heights = terrain2.terrainData.GetHeights(terrain2XBase, terrain2YBase, seamWidth, seamHeight);
            float[,] avgheights = new float[seamHeight, seamWidth];

            bool withinMaxDistance = false;

            for (int x = 0; x < avgheights.GetLength(0); x++)
            {
                for (int y = 0; y < avgheights.GetLength(1); y++)
                {
                    if (Mathf.Abs(terrain1Heights[x, y] - terrain2Heights[x, y]) < maxDifference)
                    {
                        withinMaxDistance = true;
                    }
                    avgheights[x, y] = (terrain1Heights[x, y] * terrain1Weight + terrain2Heights[x, y] * terrain2Weight) / (terrain1Weight + terrain2Weight);
                    //avgheights[x, y] = Mathf.Lerp(terrain1Heights[x, y], terrain2Heights[x, y], terrain2Weight);
                    //avgheights[x,y] = Mathf.Min(terrain1Heights[x,y], terrain2Heights[x,y]);
                }
            }
            if (withinMaxDistance)
            {
                if (applyTo1)
                {
                    terrain1.terrainData.SetHeights(terrain1XBase, terrain1YBase, avgheights);
                }
                if (applyTo2)
                {
                    terrain2.terrainData.SetHeights(terrain2XBase, terrain2YBase, avgheights);
                }
            }
            return withinMaxDistance;
        }

        /// <summary>
        /// Resizes the terrain details to a new resolution setting.
        /// </summary>
        /// <param name="terrainData"></param>
        /// <param name="targetDetailRes"></param>
        /// <param name="resolutionPerPatch"></param>
        public static void ResizeTerrainDetails(Terrain terrain, int targetDetailRes, int resolutionPerPatch)
        {
            TerrainData terrainData = terrain.terrainData;

            if (targetDetailRes == terrainData.detailResolution)
            {
                var layers = new List<int[,]>();
                for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
                    layers.Add(terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, i));

                terrainData.SetDetailResolution(targetDetailRes, resolutionPerPatch);

                for (int i = 0; i < layers.Count; i++)
                    terrainData.SetDetailLayer(0, 0, i, layers[i]);
            }
            else
            {
                terrainData.SetDetailResolution(targetDetailRes, resolutionPerPatch);
            }
        }

        /// <summary>
        /// Clear all the trees on all the terrains
        /// </summary>
        public static void ClearSpawns(SpawnerResourceType resourceType, ClearSpawnFor clearSpawnFor, ClearSpawnFrom clearSpawnFrom, List<string> terrainNames = null, Spawner spawner = null)
        {
            if (terrainNames == null)
            {
                if (clearSpawnFor == ClearSpawnFor.AllTerrains)
                {
                    if (GaiaUtils.HasDynamicLoadedTerrains())
                    {
                        GaiaSessionManager sessionManager = GaiaSessionManager.GetSessionManager();
                        terrainNames = TerrainLoaderManager.TerrainScenes.Select(x => x.GetTerrainName()).ToList();
                    }
                    else
                    {
                        terrainNames = Terrain.activeTerrains.Select(x => x.name).ToList();
                    }
                }
                else
                {
                    terrainNames = new List<string> { spawner.GetCurrentTerrain().name };
                }
            }

            string progressBarTitle = "Clearing...";

            Action<Terrain> act = null;
            switch (resourceType)
            {
                case SpawnerResourceType.TerrainTexture:
                    progressBarTitle = "Clearing Textures";
                    //Not supported, should not be required
                    throw new NotSupportedException("Clearing of Textures is currently not supported via the terrain helper");
                case SpawnerResourceType.TerrainDetail:
                    progressBarTitle = "Clearing Terrain Details";
                    act = (t) => ClearDetailsOnSingleTerrain(t, spawner, clearSpawnFrom);
                    break;
                case SpawnerResourceType.TerrainTree:
                    progressBarTitle = "Clearing Trees";
                    act = (t) => ClearTreesOnSingleTerrain(t, spawner, clearSpawnFrom);
                    break;
                case SpawnerResourceType.GameObject:
                    progressBarTitle = "Clearing Game Objects";
                    act = (t) => ClearGameObjectsOnSingleTerrain(t, spawner, clearSpawnFrom);
                    break;
                case SpawnerResourceType.Probe:
                    progressBarTitle = "Clearing Probes";
                    act = (t) => ClearGameObjectsOnSingleTerrain(t, spawner, clearSpawnFrom);
                    break;
                case SpawnerResourceType.SpawnExtension:
                    progressBarTitle = "Clearing Spawn Extensions";
                    act = (t) => ClearSpawnExtensionsOnSingleTerrain(t, spawner, clearSpawnFrom);
                    break;
                case SpawnerResourceType.StampDistribution:
                    //Not supported, should not be required
                    throw new NotSupportedException("Clearing of Stamps is currently not supported via the terrain helper");
            }

            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                GaiaUtils.CallFunctionOnDynamicLoadedTerrains(act, true, terrainNames);
            }
            else
            {
                for (int idx = 0; idx < terrainNames.Count; idx++)
                {
                    ProgressBar.Show(ProgressBarPriority.Spawning ,progressBarTitle, progressBarTitle,  idx + 1, terrainNames.Count(), true);

                    GameObject go = GameObject.Find(terrainNames[idx]);
                    if (go != null)
                    {
                        Terrain terrain = go.GetComponent<Terrain>();
                        act(terrain);
                    }
                    ProgressBar.Clear(ProgressBarPriority.Spawning);

                }
            }
        }

        private static void ClearTreesOnSingleTerrain(Terrain terrain, Spawner spawner, ClearSpawnFrom clearSpawnFrom)
        {
            if (spawner == null || clearSpawnFrom == ClearSpawnFrom.AnySource)
            {
                //No tree prototypes passed in => we delete any tree from any source
                terrain.terrainData.treeInstances = new TreeInstance[0];
            }
            else
            {
                //We need to get the correct prototype Ids for this terrain only
                //Prototype Ids might be different from terrain to terrain, depending on when / how the prototype was added
                List<int> treePrototypeIds = new List<int>();
                foreach (SpawnRule sr in spawner.m_settings.m_spawnerRules)
                {
                    if (sr.m_resourceType == GaiaConstants.SpawnerResourceType.TerrainTree)
                    {
                        int treePrototypeIndex = spawner.m_settings.m_resources.PrototypeIdxInTerrain(sr.m_resourceType, sr.m_resourceIdx, terrain);
                        if (treePrototypeIndex != -1)
                        {
                            treePrototypeIds.Add(treePrototypeIndex);
                        }
                    }
                }
                //Reapply the tree instances on this terrain, but leave all "to be deleted" ids out via the where clause
                terrain.terrainData.SetTreeInstances(terrain.terrainData.treeInstances.Where(x => !treePrototypeIds.Contains(x.prototypeIndex)).ToArray(), true);

            }
            terrain.Flush();
        }

        /// <summary>
        /// Clear all the details (grass) on all the terrains
        /// </summary>
        private static void ClearDetailsOnSingleTerrain(Terrain terrain, Spawner spawner, ClearSpawnFrom clearSpawnFrom)
        {
            int[,] details = new int[terrain.terrainData.detailWidth, terrain.terrainData.detailHeight];

            if (spawner == null || clearSpawnFrom == ClearSpawnFrom.AnySource)
            {
                for (int dtlIdx = 0; dtlIdx < terrain.terrainData.detailPrototypes.Length; dtlIdx++)
                {
                    terrain.terrainData.SetDetailLayer(0, 0, dtlIdx, details);
                }
            }
            else
            {
                //We need to get the correct prototype Ids for this terrain only
                //Prototype Ids might be different from terrain to terrain, depending on when / how the prototype was added
                List<int> detailPrototypeIds = new List<int>();
                foreach (SpawnRule sr in spawner.m_settings.m_spawnerRules)
                {
                    if (sr.m_resourceType == GaiaConstants.SpawnerResourceType.TerrainDetail)
                    {
                        int detailPrototypeIndex = spawner.m_settings.m_resources.PrototypeIdxInTerrain(sr.m_resourceType, sr.m_resourceIdx, terrain);
                        if (detailPrototypeIndex != -1)
                        {
                            detailPrototypeIds.Add(detailPrototypeIndex);
                        }
                    }
                }

                for (int dtlIdx = 0; dtlIdx < detailPrototypeIds.Count; dtlIdx++)
                {
                    terrain.terrainData.SetDetailLayer(0, 0, detailPrototypeIds[dtlIdx], details);
                }
            }
            terrain.Flush();
        }


        private static void ClearGameObjectsOnSingleTerrain(Terrain terrain, Spawner spawner, ClearSpawnFrom clearSpawnFrom)
        {
            Spawner[] allAffectedSpawners;

            if (clearSpawnFrom == ClearSpawnFrom.OnlyThisSpawner)
            {
                allAffectedSpawners = new Spawner[1] { spawner };
            }
            else
            {
                allAffectedSpawners = Resources.FindObjectsOfTypeAll<Spawner>();
            }

            foreach (Spawner sp in allAffectedSpawners)
            {
                foreach (SpawnRule sr in sp.m_settings.m_spawnerRules.Where(x=>x.m_resourceType == SpawnerResourceType.GameObject || x.m_resourceType == SpawnerResourceType.SpawnExtension || x.m_resourceType == SpawnerResourceType.Probe))
                    Spawner.ClearGameObjectsForRule(sp, sr, false, terrain);
            }
        }

        private static void ClearSpawnExtensionsOnSingleTerrain(Terrain terrain, Spawner spawner, ClearSpawnFrom clearSpawnFrom)
        {
            Spawner[] allAffectedSpawners;

            if (clearSpawnFrom == ClearSpawnFrom.OnlyThisSpawner)
            {
                allAffectedSpawners = new Spawner[1] { spawner };
            }
            else
            {
                allAffectedSpawners = Resources.FindObjectsOfTypeAll<Spawner>();
            }

            foreach (Spawner sp in allAffectedSpawners)
            {
                foreach (SpawnRule sr in sp.m_settings.m_spawnerRules)
                {
                    sp.ClearSpawnExtensionsForRule(sr, sp.m_settings);
                    Spawner.ClearGameObjectsForRule(sp, sr, false, terrain);
                }
            }
        }

        /// <summary>
        /// Get the range from the terrain
        /// </summary>
        /// <returns></returns>
        public static float GetRangeFromTerrain()
        {
            Terrain t = Gaia.TerrainHelper.GetActiveTerrain();
            if (t != null)
            {
                return Mathf.Max(t.terrainData.size.x, t.terrainData.size.z) / 2f;
            }
            return 0f;
        }

        /// <summary>
        /// Returns true if this terrain is a world map terrain.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsWorldMapTerrain(Terrain t)
        {
            return t.name.StartsWith(GaiaConstants.worldMapTerrainPrefix);
        }

        public static bool IsWorldMapTerrain(TerrainData td)
        {
            return td.name.StartsWith(GaiaConstants.worldMapTerrainPrefix);
        }

        /// <summary>
        /// Returns terrain names of terrains that intersect with the given bounds object
        /// </summary>
        /// <param name="bounds">A bounds object to check against the terrains. Needs to be in absolute world space position, mind the current origin offset!</param>
        /// <returns></returns>
        public static string[] GetTerrainsIntersectingBounds(BoundsDouble bounds)
        {
            //Reduce the bounds size a bit to prevent selecting terrains that are perfectly aligned with the bounds border
            //-this leads to too many terrains being logged as affected by an operation otherwise.
            Bounds intersectingBounds = new BoundsDouble();
            intersectingBounds.center = bounds.center;
            intersectingBounds.size = bounds.size - new Vector3Double(0.001f, 0.001f, 0.001f);

            if (GaiaUtils.HasDynamicLoadedTerrains())
            {
                GaiaSessionManager sessionManager = GaiaSessionManager.GetSessionManager();
                if (sessionManager == null)
                {
                    Debug.LogError("Trying to get terrains that intersect with bounds, but there is no session manager in scene.");
                    return null;
                }
                return TerrainLoaderManager.TerrainScenes.Where(x => x.m_bounds.Intersects(intersectingBounds)).Select(x => x.GetTerrainName()).ToArray();
            }
            else
            {
                List<string> affectedTerrainNames = new List<string>();
                foreach (Terrain t in Terrain.activeTerrains)
                {
                    if (intersectingBounds.Intersects(TerrainHelper.GetWorldSpaceBounds(t)))
                    {
                        affectedTerrainNames.Add(t.name);
                    }
                }
                return affectedTerrainNames.ToArray();
            }
        }

        public static TerrainLayer GetLayerFromPrototype(ResourceProtoTexture proto)
        {
            foreach (Terrain t in Terrain.activeTerrains)
            {
                foreach (TerrainLayer layer in t.terrainData.terrainLayers)
                {
                    if (proto != null && layer != null)
                    {
                        if (proto.m_texture != null && layer.diffuseTexture != null)
                        {
                            if (PWCommon5.Utils.IsSameTexture(proto.m_texture, layer.diffuseTexture, false) == true)
                            {
                                return layer;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static int GetTreePrototypeIDFromSpawnRule(SpawnRule sr, Terrain terrain)
        {
            Spawner spawner = CollisionMask.m_allTreeSpawners.FirstOrDefault(x => x.m_settings.m_spawnerRules.Contains(sr));
            if (spawner != null)
            {
                GameObject treePrefabInRule = spawner.m_settings.m_resources.m_treePrototypes[sr.m_resourceIdx].m_desktopPrefab;
                for (int i = 0; i < terrain.terrainData.treePrototypes.Length; i++)
                {
                    TreePrototype tp = terrain.terrainData.treePrototypes[i];
                    if (tp.prefab == treePrefabInRule)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static Vector3 GetWorldCenter(bool sampleHeight = false)
        {
            BoundsDouble bounds = new BoundsDouble();
            GetTerrainBounds(ref bounds);

            if (sampleHeight)
            {
                Terrain t = GetTerrain(bounds.center);
                if (t != null)
                {
                    Vector3 centerOnTerrain = t.transform.position + new Vector3(t.terrainData.size.x / 2f, 0f, t.terrainData.size.z / 2f);
                    float height = t.SampleHeight(centerOnTerrain);
                    return new Vector3(centerOnTerrain.x, height, centerOnTerrain.z);
                }
                else
                {
                    //No terrain? The user might be using mesh terrains then. Send out a raycast at the center to determine height
                    RaycastHit raycastHit = new RaycastHit();
                    if (Physics.Raycast(new Vector3Double(bounds.center.x, 1000000f, bounds.center.z), Vector3.down, out raycastHit))
                    {
                        return raycastHit.point;
                    }
                    else
                    {
                        return bounds.center;
                    }
                }
            }
            else
            {
                return new Vector3((float)bounds.center.x, (float)bounds.center.y, (float)bounds.center.z);
            }
        }
    }
}