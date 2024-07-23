using System.Collections.Generic;
using Core;
using Photography;
using UnityEngine;
using UnityEngine.UI;

namespace AlbumSystem
{
    public class AlbumManager : MonoBehaviour
    {
        
        [SerializeField] AlbumPhoto albumPhotoPrefab;
        PhotoManager _photoManager;
        [SerializeField] GridLayoutGroup gridLayout;
        Dictionary<AlbumPhoto, PhotoData> photoDictionary = new ();

        private void Awake()
        {
            if (gridLayout is null)
                gridLayout = GetComponentInChildren<GridLayoutGroup>();
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
            
            if (albumPhotoPrefab is null)
            {
                Debug.LogError("AlbumPhoto prefab is null");
                return;
            }
            

            if (photoDictionary.ContainsValue(photoData))
            {
                Debug.LogWarning("Photo already exists in album");
                return;
            }
            
            AlbumPhoto photo = Instantiate(albumPhotoPrefab, gridLayout.transform);
            photoDictionary.Add(photo, photoData);
            photo.SetSprite(SpriteFromTexture(photoData.Photo));
        }
        
        private void LoadPhotos(List<PhotoData> photoData)
        {
            ClearPhotos();
            
            foreach (var photo in photoData)
            {
                AddAlbumPhoto(photo);
                Debug.Log("Photo added to album");
            }

            Debug.Log(null == photoDictionary ? "Photo Dictionary is null" : "Photo Dictionary is not null");
        }
        
        public void ClearPhotos()
        {
            foreach (var photo in photoDictionary.Keys)
            {
                Destroy(photo.gameObject);
            }
            photoDictionary.Clear();
        }
        
        
        
        public static Sprite SpriteFromTexture(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
        
        
    }
}
