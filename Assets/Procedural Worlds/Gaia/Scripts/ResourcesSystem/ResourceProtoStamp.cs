using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gaia
{
    public enum BorderMaskStyle {ImageMask, DistanceMask, None}


    /// <summary>
    /// Prototype for stamps and their fitness
    /// </summary>
    [System.Serializable]
    public class ResourceProtoStamp
    {
        /// <summary>
        /// The stamp directory name where stamps for this feature will be pulled from
        /// </summary>
        public string m_featureType;
        /// <summary>
        /// The influence setting for the stamp.
        /// </summary>
        public ImageMaskInfluence m_stampInfluence = ImageMaskInfluence.Global;
        /// <summary>
        /// The secondary mask used in the stamp to blend the stamp borders with the terrain.
        /// </summary>
        public BorderMaskStyle m_borderMaskStyle = BorderMaskStyle.DistanceMask;
        /// <summary>
        /// The directory name where stamps for the secondary image mask will be pulled from
        /// </summary>
        public string m_borderMaskType = "Masks";
        /// <summary>
        /// The operation to perform for this stamp
        /// </summary>
        public GaiaConstants.TerrainGeneratorFeatureOperation m_operation = GaiaConstants.TerrainGeneratorFeatureOperation.MixHeight;
        /// <summary>
        /// only used internally to calculate the spawn chances during spawning
        /// </summary>
        public float m_stackedChance;
        /// <summary>
        /// The minimum width of the stamp
        /// </summary>
        public float m_minWidth = 50f;
        /// <summary>
        /// The maximum width of the stamp
        /// </summary>
        public float m_maxWidth = 200f;
        /// <summary>
        /// Whether the width should be tied to the strength of the spawn mask. If not, it will be rolled randomly.
        /// </summary>
        public bool m_tieWidthToStrength = true;
        /// <summary>
        /// The minimum height of the stamp
        /// </summary>
        public float m_minHeight = 1f;
        /// <summary>
        /// The maximum height of the stamp
        /// </summary>
        public float m_maxHeight = 5f;
        /// <summary>
        /// The minimum strength for a mix operation 
        /// </summary>
        public float m_minMixStrength = 0.10f;
        /// <summary>
        /// The maximum strength for a mix operation 
        /// </summary>
        public float m_maxMixStrength = 0.25f;
        /// <summary>
        /// The minimum mid point for a mix operation 
        /// </summary>
        public float m_minMixMidPoint = 0.25f;
        /// <summary>
        /// The maximum mid point for a mix operation 
        /// </summary>
        public float m_maxMixMidPoint = 0.75f;

        /// <summary>
        /// Whether the height should be tied to the strength of the spawn mask. If not, it will be rolled randomly.
        /// </summary>
        public bool m_tieHeightToStrength = true;
        /// <summary>
        /// The minimum height difference from the terrain surface of the stamp
        /// </summary>
        public float m_minYOffset = -1f;
        /// <summary>
        /// The maximum height difference from the terrain surface of the stamp
        /// </summary>
        public float m_maxYOffset = 1f;
        /// <summary>
        /// The chance that the stamp will be inverted
        /// </summary>
        public float m_invertChance = 0f;
        /// <summary>
        /// A mapping between the input height for the random terrain generation and the final stamp height that will be applied
        /// </summary>
        public AnimationCurve m_inputHeightToStampHeightMapping = ImageMask.NewAnimCurveStraightUpwards();
        /// <summary>
        /// A mapping between the input height for the random terrain generation and the probability that this stamp will appear
        /// </summary>
        public AnimationCurve m_inputHeightToProbabilityMapping = ImageMask.NewAnimCurveStraightUpwards();
        /// <summary>
        /// The overall spawn probability for this stamp
        /// </summary>
        public float m_spawnProbability;
       
    }
}