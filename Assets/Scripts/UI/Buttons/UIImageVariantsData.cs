using System.Collections.Generic;
using UnityEngine;

namespace UI.Buttons
{
    [CreateAssetMenu(fileName = "UIImageVariantsData", menuName = "UI/UIImageVariantsData", order = 0)]
    public class UIImageVariantsData
        : ScriptableObject
    {
        [SerializeField] List<ImageVariants> ImageDetailVariants;
        
        public Sprite GetSprite(Variant variant)
        {
            foreach (var imageDetailVariant in ImageDetailVariants)
            {
                if (imageDetailVariant.variant == variant)
                {
                    return imageDetailVariant.Sprite;
                }
            }
            return null;
        }
        
        
    }
}