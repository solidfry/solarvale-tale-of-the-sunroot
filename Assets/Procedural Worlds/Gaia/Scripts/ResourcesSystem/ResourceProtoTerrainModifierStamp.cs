using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gaia
{
    /// <summary>
    /// Prototype for terrain modifier stamps, stamper se
    /// </summary>
    [System.Serializable]
    public class ResourceProtoTerrainModifierStamp
    {
        /// <summary>
        /// The name for the terrain modifier stamp
        /// </summary>
        public string m_name;

        /// <summary>
        /// Stamp y location
        /// </summary>
        public double m_y = 50;

        /// <summary>
        /// Stamp height - this is the vertical scaling factor
        /// </summary>
        public float m_height = 10f;

        /// <summary>
        /// The absolute added / subtracted height for add / subtract height operations
        /// </summary>
        public float m_absoluteHeightValue;

        /// <summary>
        /// Stamp rotation
        /// </summary>
        public float m_rotation = 0f;

        /// <summary>
        /// The strength of the blending operation in the stamper
        /// </summary>
        public float m_blendStrength = 0.5f;

        /// <summary>
        /// The current operation that the stamper will perform on the terrain when pressing the stamp button
        /// </summary>
        public GaiaConstants.FeatureOperation m_operation = GaiaConstants.FeatureOperation.RaiseHeight;

        /// <summary>
        /// height transform curve when using Effects > Height Transform
        /// </summary>
        public AnimationCurve m_heightTransformCurve = ImageMask.NewAnimCurveStraightUpwards();

        /// <summary>
        /// size of the increased features when using Effects>Contrast
        /// </summary>
        public float m_contrastFeatureSize = 10;

        /// <summary>
        /// strength of the contrast effect when using Effects>Contrast
        /// </summary>
        public float m_contrastStrength = 2;

        /// <summary>
        /// size of the features being included in a terrace when using Effects>Terraces
        /// </summary>
        public float m_terraceCount = 100f;

        /// <summary>
        /// Added Jitter when using Effects>Terraces
        /// </summary>
        public float m_terraceJitterCount = 0.5f;

        /// <summary>
        /// Bevel Amount when using Effects>Terraces
        /// </summary>
        public float m_terraceBevelAmountInterior;

        /// <summary>
        /// Sharpness when using Effects>Sharpen Ridges
        /// </summary>
        public float m_sharpenRidgesMixStrength = 0.5f;

        /// <summary>
        /// Erosion Amount when using Effects>Sharpen Ridges
        /// </summary>
        public float m_sharpenRidgesIterations = 16f;

        public float m_powerOf;
        public float m_smoothVerticality = 0f;
        public float m_smoothBlurRadius = 10f;


        /// <summary>
        /// A fixed Image masked used as input for some of the operations.
        /// </summary>
        public ImageMask m_stamperInputImageMask = new ImageMask();

        /// <summary>
        /// The mix level of the stamp for the mix height operation.
        /// </summary>
        public float m_mixMidPoint = 0.5f;

        /// <summary>
        /// The strength of the mix height operation.
        /// </summary>
        public float m_mixHeightStrength = 0.5f;

    }
}