// Copyright © 2018 Procedural Worlds Pty Limited.  All Rights Reserved.
using UnityEngine;
using System.Collections.Generic;
using static Gaia.GaiaConstants;
using UnityEditor;



namespace Gaia
{
    public enum NoiseTypeName { Billow, Perlin, Ridge, Value }

    public enum BaseTerrainInputType { Generator, Image, ExistingTerrain }

    /// <summary>
    /// Stores data for a noise layer used in the shape generation in the world designer
    /// </summary>
    [System.Serializable]
    public class BaseTerrainNoiseSettings
    {
        public NoiseTypeName m_shapeNoiseStyle = NoiseTypeName.Perlin;
        public float m_shapeSize = 8f;
        public float m_shapeStrength = 0.3f;
        public float m_shapeSteepness = 0.2f;
        public float m_shapeGranularity = 1.5f;
        public Vector3 m_shapeOffset;
        public AnimationCurve m_noiseStrengthTransform = ImageMask.NewAnimCurveBaseTerrainGenerator();
        public float m_warpIterations = 0.0f;
        public float m_warpStrength = 1.0f;
        public Vector4 m_warpOffset;
    }

    /// <summary> Contains settings for the initial creation of the Base Terrain during random terrain generation</summary>
    [System.Serializable]
    public class BaseTerrainSettings : ScriptableObject, ISerializationCallbackReceiver
    {

        #region Public Variables

        /// <summary>
        /// If the baseTerrain should be displayed as a preview in the random terrain generator or not.
        /// </summary>
        public bool m_drawPreview = true;

        /// <summary>
        /// If the world designer should always render the full preview immediately after a new world was created
        /// </summary>
        public bool m_alwaysFullPreview = false;

        public BaseTerrainInputType m_baseTerrainInputType = BaseTerrainInputType.Generator;
        public float m_heightScale = 7f;
        public float m_baseLevel = 0f;
        public float m_heightVariance = 0.5f;
        public GeneratorBorderStyle m_borderStyle = GeneratorBorderStyle.Water;
        public ImageMask m_inputImageMask = new ImageMask();
        public BaseTerrainNoiseSettings[] m_baseNoiseSettings = new BaseTerrainNoiseSettings[1] { new BaseTerrainNoiseSettings() }; //, new BaseTerrainNoiseSettings() };
        public Terrain m_inputTerrain;
        public bool m_advancedInputSettingsUnfolded = false;


        #endregion
        #region Serialization

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
        }

        #endregion


   

    }
}
