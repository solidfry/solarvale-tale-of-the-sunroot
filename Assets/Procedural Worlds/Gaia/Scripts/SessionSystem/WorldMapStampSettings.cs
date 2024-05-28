
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Gaia
{
    [System.Serializable]
    public class WorldMapStampSettings : ScriptableObject
    {
        /// <summary>
        /// The size of the world map preview from the terrain generator we are stamping from
        /// </summary>
        public int m_worldMapSize = 2048;

        /// <summary>
        /// The target total world size we are stamping to
        /// </summary>
        public int m_targetWorldSize = 2048;

        /// <summary>
        /// the number of tiles in rows / colums to build the world when multi-terrain is being used
        /// </summary>
        public int m_tiles = 1;

        /// <summary>
        /// The tilesize used for the world creation / stamping
        /// </summary>
        public int m_tilesize = 1024;

        /// <summary>
        /// The tile height used for the world creation / stamping
        /// </summary>
        public float m_tileHeight = 1024;

        /// <summary>
        /// The heightmap resolution used for the world creation / stamping
        /// </summary>
        public int m_heightmapResolution = 1025;


        /// <summary>
        /// The minimum, overall world height for the final terrain. We know this before exporting from the World Map Preview. This value is then used during stamping to make sure the 
        /// output is consistent for all individual stamps.
        /// </summary>
        public float m_minWorldHeight = 0f;

        /// <summary>
        /// The maximum, overall world height for the final terrain. We know this before exporting from the World Map Preview. This value is then used during stamping to make sure the 
        /// output is consistent for all individual stamps.
        /// </summary>
        public float m_maxWorldHeight = 1024f;

        /// <summary>
        /// The stamper settings for the base terrain in the terrain generator
        /// </summary>
        public StamperSettings m_baseTerrainStamperSettings;

        /// <summary>
        /// The list of concatenated spawned stamper settings that makes the complete preview from the terrain generator. 
        /// </summary>
        public List<StamperSettings> m_stamperSettingsList;


    }
}