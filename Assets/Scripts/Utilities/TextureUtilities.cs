using System.IO;
using UnityEngine;

namespace Utilities
{
    public static class TextureUtilities
    {
        public static string SaveDirectory => Path.Combine(Application.persistentDataPath, "Photos");

        public static void SaveTextureToFile(Texture2D texture, string fileName)
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            byte[] bytes = texture.EncodeToPNG();
            string filePath = Path.Combine(SaveDirectory, fileName + ".png");
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Saved texture to: " + filePath);
        }

        public static Texture2D LoadTextureFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                Debug.Log("Loaded texture from: " + filePath);
                return texture;
            }
            else
            {
                Debug.LogWarning("Texture file not found: " + filePath);
                return null;
            }
        }
    }
}