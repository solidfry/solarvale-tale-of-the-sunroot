using System.Collections.Generic;
using AlbumSystem;
using Core;
using Photography;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Album
{
    public class AlbumUIController : MonoBehaviour
    {
        [SerializeField]
        PhotoManager _photoManager;
        [SerializeField] SpriteSetter spriteSetterPrefab;
        [FormerlySerializedAs("gridLayout")] [SerializeField] GridLayoutGroup gridLayoutGroup;
        readonly Dictionary<SpriteSetter, PhotoData> _photoDictionary = new ();

        private void Awake()
        {
            if (gridLayoutGroup is null)
                gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();
        }

        private void Start()
        {
            _photoManager = FindObjectOfType<GameManager>().PhotoManager;
            
            if (_photoManager.GetPhotos().Count > 0)
                LoadPhotos(_photoManager.GetPhotos());
            
            _photoManager.OnPhotoAdded += AddAlbumPhoto;
            _photoManager.OnPhotosLoaded += LoadPhotos;
        }

        private void OnDisable()
        {
            _photoManager.OnPhotosLoaded -= LoadPhotos;
            _photoManager.OnPhotoAdded -= AddAlbumPhoto;
        }

        void AddAlbumPhoto(PhotoData photoData)
        {
            
            if (spriteSetterPrefab is null)
            {
                Debug.LogError("AlbumPhoto prefab is null");
                return;
            }
            

            if (_photoDictionary.ContainsValue(photoData))
            {
                Debug.LogWarning("Photo already exists in album");
                return;
            }
            
            SpriteSetter photo = Instantiate(spriteSetterPrefab, gridLayoutGroup.transform);
            _photoDictionary.Add(photo, photoData);
            photo.SetSprite(SpriteFromTexture(photoData.Photo));
        }
        
        private void LoadPhotos(List<PhotoData> photoData)
        {
            ClearPhotos();
            
            foreach (var photo in photoData)
            {
                AddAlbumPhoto(photo);
                // Debug.Log("Photo added to album");
            }

            // Debug.Log(null == _photoDictionary ? "Photo Dictionary is null" : "Photo Dictionary is not null");
        }
        
        public void ClearPhotos()
        {
            foreach (var photo in _photoDictionary.Keys)
            {
                Destroy(photo.gameObject);
            }
            _photoDictionary.Clear();
        }
        
        
        
        public static Sprite SpriteFromTexture(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
        
        
    }
}
