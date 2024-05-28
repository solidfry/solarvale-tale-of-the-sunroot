#if FLORA_PRESENT
using ProceduralWorlds.Flora;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gaia
{
    /// <summary>
    /// Data structure to provide override values for the asset guid / instance ID for a Flora LOD entry. List of those overrides are being used to handle loading of multiple LODs per spawn rule.
    /// </summary>
    public class FloraLODIdOverrides
    {
        public string m_assetGUIDOverride;
        public int m_instanceIDOverride;
    }


    /// <summary>
    /// Data structure to support serializing multiple LODs of flora setting objects for trees / terrain details together with tree / terrain detail resource settings in the spawner
    /// </summary>
    [System.Serializable]
    public class FloraLOD
    {
        public int m_index = 0;
        public bool m_foldedOut = false;
        public GaiaConstants.SpawnerResourceType m_spawnerResourceType;
        public string m_name = "New Flora Settings Object";
        public string m_detailerSettingsObjectAssetGUID;
        public int m_detailerSettingsObjectInstanceID;
#if FLORA_PRESENT
        public DetailScriptableObject DetailerSettingsObject
        {
            get
            {
                if (m_pwDetailerSettingsObject == null)
                {
                    if (!(m_spawnerResourceType == GaiaConstants.SpawnerResourceType.TerrainDetail || m_spawnerResourceType == GaiaConstants.SpawnerResourceType.TerrainTree))
                    {
                        Debug.LogError($"Trying to create a Flora Scriptable object for a {m_spawnerResourceType} resource, this is not supported (yet)");
                        return null;
                    }


                    if (!string.IsNullOrEmpty(m_detailerSettingsObjectAssetGUID))
                    {
#if UNITY_EDITOR
                        string assetPath = AssetDatabase.GUIDToAssetPath(m_detailerSettingsObjectAssetGUID);

                        //This can be multiple objects since the scriptable object can be embedded in a spawner settings file
                        UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                        //Looking for the object via the instance ID
                        foreach (UnityEngine.Object obj in allObjects)
                        {
                            if (obj.GetType() == typeof(DetailScriptableObject) && obj.GetInstanceID() == m_detailerSettingsObjectInstanceID)
                            {
                                //found it, cast it into the correct type to store it
                                m_pwDetailerSettingsObject = (DetailScriptableObject)obj;
                                //is this a file that was embedded within other assets at this path?
                                if (allObjects.Count() > 1)
                                {
                                    //we need to create a copy and save it to not work with the embedded file
                                    m_pwDetailerSettingsObject = ScriptableObject.Instantiate(m_pwDetailerSettingsObject);
                                    FloraUtils.SaveSettingsFile(m_pwDetailerSettingsObject, ref m_detailerSettingsObjectAssetGUID, ref m_detailerSettingsObjectInstanceID, false, m_name + "_" + m_index.ToString(), GaiaDirectories.GetFloraDataPath());
                                }
                            }
                        }
#endif
                    }
                }
                return m_pwDetailerSettingsObject;
            }
            set
            {
                if (value != m_pwDetailerSettingsObject)
                {
                    m_pwDetailerSettingsObject = value;
                    if (value != null)
                    {
#if UNITY_EDITOR
                        m_detailerSettingsObjectAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value));
                        m_detailerSettingsObjectInstanceID = value.GetInstanceID();
#endif
                    }
                    else
                    {
                        m_detailerSettingsObjectAssetGUID = "";
                        m_detailerSettingsObjectInstanceID = -1;
                    }
                }
            }
        }
        //This needs to be serialized still to survive recompiles, etc.!
        [SerializeField]
        private DetailScriptableObject m_pwDetailerSettingsObject;

        /// <summary>
        /// Creates an instantiated (separate) copy of the detailer settings object in the Gaia User Data directory.
        /// </summary>
        public void InstantiateDetailerSettingsGO(ref Dictionary<int, string> materialMap)
        {
#if UNITY_EDITOR
            if (m_pwDetailerSettingsObject == null)
            {
                if (!string.IsNullOrEmpty(m_detailerSettingsObjectAssetGUID))
                {

                    string assetPath = AssetDatabase.GUIDToAssetPath(m_detailerSettingsObjectAssetGUID);

                    //This can be multiple objects since the scriptable object can be embedded in a spawner settings file
                    UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                    //Looking for the object via the instance ID
                    foreach (UnityEngine.Object obj in allObjects)
                    {
                        if (obj.GetType() == typeof(DetailScriptableObject) && obj.GetInstanceID() == m_detailerSettingsObjectInstanceID)
                        {
                            //found it, cast it into the correct type to store it
                            m_pwDetailerSettingsObject = (DetailScriptableObject)obj;
                        }
                    }
                }
            }

            if (m_pwDetailerSettingsObject == null)
            {
                return;
            }

            string floraDataPath = GaiaDirectories.GetFloraDataPath();

            if (m_pwDetailerSettingsObject != null)
            {
                m_pwDetailerSettingsObject = ScriptableObject.Instantiate(m_pwDetailerSettingsObject);
                FloraUtils.SaveSettingsFile(m_pwDetailerSettingsObject, ref m_detailerSettingsObjectAssetGUID, ref m_detailerSettingsObjectInstanceID, false, m_name + "_LOD" + m_index.ToString(), floraDataPath);
            }

            Material[] mats = m_pwDetailerSettingsObject.Mat.Where(x => x != null).ToArray();
            List<Material> createdMaterials = new List<Material>();
            //Handle Materials - those should never be null in theory, just need to be re-instantiated and saved
            for (int i = 0; i < mats.Length; i++)
            {
                //if the material map contains the instance ID already, this means this material has already been created within the Flora data folder
                int instanceID = m_pwDetailerSettingsObject.Mat[i].GetInstanceID();
                if (materialMap.Keys.Contains(instanceID))
                {
                    //-> take the already existing material then!
                    m_pwDetailerSettingsObject.Mat[i] = (Material)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(materialMap.First(x => x.Key == instanceID).Value), typeof(Material));
                }
                else
                {
                    //create a new material
                    string originalName = m_pwDetailerSettingsObject.Mat[i].name;
                    m_pwDetailerSettingsObject.Mat[i] = Material.Instantiate(m_pwDetailerSettingsObject.Mat[i]);
                    //Remove "(Clone)" from the name
                    m_pwDetailerSettingsObject.Mat[i].name = originalName;
                    string assetPath = floraDataPath + "/" + m_pwDetailerSettingsObject.Mat[i].name + "_" + instanceID + ".mat";
                    AssetDatabase.CreateAsset(m_pwDetailerSettingsObject.Mat[i], assetPath);
                    //AssetDatabase.ImportAsset(assetPath);
                    //Log the creation of the material for this instance ID in the material map
                    materialMap.Add(instanceID,AssetDatabase.AssetPathToGUID(assetPath));
                    //createdMaterials.Add((Material)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Material)));
                    createdMaterials.Add(m_pwDetailerSettingsObject.Mat[i]);
                }
            }

            //Migrate shaders in materials (could potentially be created in different pipeline originally)
            GaiaUtils.ProcessMaterialLibrary(createdMaterials);
#endif
        }

        /// <summary>
        /// Copies the fields of the old object over into the existing one to overwrite it
        /// </summary>
        /// <param name="oldObject"></param>
        public void CopySettingsAndApply(DetailScriptableObject oldObject)
        {
            string oldName = oldObject.name;
            GaiaUtils.CopyFields(m_pwDetailerSettingsObject, oldObject, true);
            DetailerSettingsObject = oldObject;
            DetailerSettingsObject.name = oldName;
            DetailerSettingsObject.SourceDataType = CoreCommonFloraData.SourceDataType.Detail;
            //Make sure the materials use the right shader
            GaiaUtils.ProcessMaterialLibrary(DetailerSettingsObject.Mat.ToList());
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }

#endif
        }
}

