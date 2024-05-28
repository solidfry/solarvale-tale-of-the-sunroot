using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gaia
{


    /// <summary>
    /// The baked mask types publicly available for selection
    /// </summary>
    public enum BakedMaskType { LayerGameObject = 2, LayerTree = 3, RadiusTag=1, RadiusTree = 0}

    /// <summary>
    /// All possible baked mask types
    /// </summary>
    public enum BakedMaskTypeInternal { RadiusTree=0, RadiusTag=1, WorldBiomeMask=2, LayerGameObject=3, LayerTree=4 }
    [System.Serializable]
    public class CollisionMask
    {
        public bool m_active = true;
        public bool m_invert = false;
        public BakedMaskType m_type;
        //public int m_treePrototypeId= -99;

        //We internally store the tree spawn rule guids in a list, but use a mask for multiselect in the editor
        public List<string> m_selectedTreeSpawnRuleGUIDs = new List<string>();
        private List<int> m_radiusTreeMaskValues = new List<int>() { 0 };
        public List<int> RadiusTreeMaskValues
        {
            get {
                UpdateRadiusTreeBitMasks();
                return m_radiusTreeMaskValues;
            }
            set
            {
                m_selectedTreeSpawnRuleGUIDs.Clear();
                for (int j = 0; j < value.Count; j++)
                {
                    int maskValue = value[j];
                    //special treatment for 0 (Nothing) and -1 (Everything)
                    if (maskValue <= 0)
                    {
                        if (maskValue == -1)
                        {
                            //each mask value represents up to 32 spawn rules - if the user selected "Everything" for the dropdown
                            //that draws those 32 rules we need to add those 32 rules
                            for (int k =0; k < Mathf.Min(32,m_allTreeSpawnRules.Length-(j*32)); k++)
                            {
                                SpawnRule sr = CollisionMask.m_allTreeSpawnRules[k+(j*32)];
                                m_selectedTreeSpawnRuleGUIDs.Add(sr.GUID);
                            }
                        }
                        continue;
                    }
                    //for any other number we iterate and set the selected rule GUIDs according to the bit mask from the int
                    BitArray flagsArray = new BitArray(new[] { maskValue });
                    for (int i = 0; i < flagsArray.Length; i++)
                    {
                        if (flagsArray[i] == true)
                        {
                            m_selectedTreeSpawnRuleGUIDs.Add(CollisionMask.m_allTreeSpawnRules[i + (j*32)].GUID);
                        }
                    }
                }
                m_radiusTreeMaskValues = value;
            }
        }


        /// <summary>
        /// Updates the bitmask values for the radius tree collision mask type according to the selected spawn rule GUIDs
        /// </summary>
        public void UpdateRadiusTreeBitMasks()
        {
            m_radiusTreeMaskValues.Clear();

            for (int k = 0; k < m_allTreeSpawnRules.Length; k += 32)
            {
                bool[] allFlags = new bool[Mathf.Min(32, CollisionMask.m_allTreeSpawnRules.Length - k)];
                for (int i = k; i < k + allFlags.Length; i++)
                {
                    if (m_selectedTreeSpawnRuleGUIDs.Contains(CollisionMask.m_allTreeSpawnRules[i].GUID))
                    {
                        allFlags[i-k] = true;
                    }
                }

                BitArray flagsArray = new BitArray(allFlags);
                int[] result = new int[1];
                flagsArray.CopyTo(result, 0);
                m_radiusTreeMaskValues.Add(result[0]);
            }
        }

        public string m_treeSpawnRuleGUID = "";
        public static SpawnRule[] m_allTreeSpawnRules;
        public static Spawner[] m_allTreeSpawners;
        public static string[] m_allTreeSpawnRuleNames;
        public static int[] m_allTreeSpawnRuleIndices;

        public string m_tag;
        public float m_Radius;
        public float m_growShrinkDistance;
        public LayerMask m_layerMask;
        public string[] m_layerMaskLayerNames;

    }
}
