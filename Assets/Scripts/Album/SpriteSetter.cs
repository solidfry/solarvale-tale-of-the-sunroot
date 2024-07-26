using UnityEngine;
using UnityEngine.UI;

namespace AlbumSystem
{
    public class SpriteSetter : MonoBehaviour
    {
        [SerializeField] Image photo;
        
        public void SetSprite(Sprite sprite)
        {
            photo.sprite = sprite;
        }
    }
}
