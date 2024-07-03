using UnityEngine;
using UnityEngine.UI;

namespace AlbumSystem
{
    public class AlbumPhoto : MonoBehaviour
    {
        [SerializeField] Image photo;
        
        public void SetSprite(Sprite sprite)
        {
            photo.sprite = sprite;
        }
    }
}
