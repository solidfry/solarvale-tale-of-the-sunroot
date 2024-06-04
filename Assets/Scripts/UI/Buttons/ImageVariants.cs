using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Buttons
{
    [Serializable]
    public class ImageVariants
    {
        [FormerlySerializedAs("levelOfDetail")] [FormerlySerializedAs("LevelOfDetail")] public Variant variant;
        public Sprite Sprite;
    }
}