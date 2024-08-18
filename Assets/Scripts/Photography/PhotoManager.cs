using System;
using System.Collections.Generic;
using System.IO;
using Events;
using UnityEngine;
using Utilities;

namespace Photography
{
    public class PhotoManager : MonoBehaviour
    {
        [SerializeField] List<PhotoData> photos = new ();
        
        public event Action<List<PhotoData>> OnPhotosLoaded;
        public event Action<PhotoData> OnPhotoAdded;
    
        private void Awake()
        {
            LoadAll();
        }
    
        private void OnEnable()
        {
            GlobalEvents.OnPhotoKeptEvent += OnPhotoKept;
        }
    
        private void OnDisable()
        {
            GlobalEvents.OnPhotoKeptEvent -= OnPhotoKept;
        }
    
        public List<PhotoData> GetPhotos()
        {
            return photos;
        }

        private void OnPhotoKept(PhotoData photo)
        {
            if (photo == null)
            {
                Debug.LogError("Photo data is null");
                return;
            }
            photos.Add(photo);
            Save(photo);
            OnPhotoAdded?.Invoke(photo);
        }
    
        private void Save(PhotoData photo)
        {
            string fileName = photo.GenerateFileName();
            TextureUtilities.SaveTextureToFile(photo.Photo, fileName);
        
            // Save meta data
            string saveDirectory = TextureUtilities.SaveDirectory;
        
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
        
            string jsonPath = Path.Combine(saveDirectory, fileName + ".json");
            File.WriteAllText(jsonPath, photo.ToJson());

            Debug.Log(this + " Saved photo to " + jsonPath);
        
        }
    
    
        private void LoadAll()
        {
            string saveDirectory = TextureUtilities.SaveDirectory;
            if (!Directory.Exists(saveDirectory))
            {
                Debug.Log("No photos saved");
                return;
            }
        
            string[] jsonFiles = Directory.GetFiles(saveDirectory, "*.json");
            foreach (var jsonPath in jsonFiles)
            {
                string json = File.ReadAllText(jsonPath);

                PhotoData photo = PhotoData.FromJson(json);
            
                string texturePath = Path.Combine(saveDirectory, photo.GenerateFileName() + ".png");
                Texture2D texture = TextureUtilities.LoadTextureFromFile(texturePath);
                if (texture != null)
                {
                    photo.SetPhoto(texture);
                    photos.Add(photo);
                }

            }
        
            // Debug.Log(this + " Loaded " + photos.Count + " photos");
            OnPhotosLoaded?.Invoke(photos);
        }
    
    }
}