using System;
using System.Collections.Generic;
using Entities;
using UnityEngine;

namespace Photography
{
    [Serializable]
    public class PhotoData
    {
        [SerializeField] private List<string> entitiesInPhoto;
        [SerializeField] private string dateTakenString;
        [NonSerialized] private DateTime dateTaken;

        [NonSerialized] private Texture2D photo;
        public Texture2D Photo => photo;

        public PhotoData(Texture2D photo, List<EntityData> entities = null)
        {
            this.photo = photo;
            dateTaken = DateTime.Now;
            dateTakenString = dateTaken.ToString("yyyy-MM-dd HH:mm:ss");
            
            if (entities == null || entities.TrueForAll(e => e == null))
            {
                entitiesInPhoto = null;
            }
            else
            {
                entitiesInPhoto = new List<string>();
                ConvertEntitiesToString(entities);
            }
        }

        private void ConvertEntitiesToString(List<EntityData> entities = null)
        {
            if (entities == null) return;
            entities.ForEach(e => entitiesInPhoto.Add(e.Name));
        }

        public string DateTakenToString()
        {
            return dateTaken.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string GenerateFileName()
        {
            return $"photo_{dateTaken:yyyyMMdd_HHmmss}";
        }

        public void SetPhoto(Texture2D texture)
        {
            photo = texture;
        }

        public string ToJson()
        {
            // Ensure dateTakenString is updated before serialization
            dateTakenString = dateTaken.ToString("yyyy-MM-dd HH:mm:ss");
            return JsonUtility.ToJson(this);
        }

        public static PhotoData FromJson(string json)
        {
            PhotoData photoData = JsonUtility.FromJson<PhotoData>(json);
            photoData.dateTaken = DateTime.ParseExact(photoData.dateTakenString, "yyyy-MM-dd HH:mm:ss", null);
            return photoData;
        }
    }
}