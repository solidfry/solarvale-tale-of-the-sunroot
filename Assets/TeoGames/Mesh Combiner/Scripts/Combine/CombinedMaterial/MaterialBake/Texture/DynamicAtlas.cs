using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial.MaterialBake.Texture {
	[Serializable]
	public class DynamicAtlas {
		public Texture2D Texture { get; private set; }
		public string Name => Texture.name;
		public Rect Rect => new(0, 0, Texture.width, Texture.height);
		public bool IsApplied { get; private set; }
		public FreeRectChoiceHeuristic Method => method;
		public float Occupancy => rectsPack.Occupancy();
		public int Length => rectsPack.usedRectangles.Count;

		[SerializeField] private FreeRectChoiceHeuristic method;
		[SerializeField] private MaxRectsBinPack rectsPack;
		[SerializeField] private List<string> names;

		public DynamicAtlas(int size, string name) : this(width: size, height: size, name: name) {
		}

		public DynamicAtlas(int width, int height, string name,
			FreeRectChoiceHeuristic method = FreeRectChoiceHeuristic.RectBestShortSideFit) {
			Texture = new Texture2D(width, height, TextureFormat.RGBA32, false) {
				name = name
			};
			this.method = method;

			rectsPack = new MaxRectsBinPack(width, height, false);
			names = new List<string>();
		}

		#region Write

		public bool Insert(Texture2D source) => Write(source);

		public void Apply() {
			Texture.Apply();
			IsApplied = true;
		}

		private bool Write(Texture2D source) {
			var newRect = rectsPack.Insert(source.width, source.height, method);
			if (newRect.height == 0) return false;

			names.Add(source.name);

			var colors = source.GetPixels();
			Texture.SetPixels((int)newRect.x, (int)newRect.y, (int)newRect.width, (int)newRect.height, colors);

			IsApplied = false;

			return true;
		}

		#endregion

		#region Read

		public SourceInfo Get(string name) {
			var index = names.FindIndex(findName => findName == name);

			if (index == -1)
				return null;

			return Get(index);
		}

		public SourceInfo this[int index] => Get(index);

		public SourceInfo Get(int index) {
			if (IsApplied == false)
				Apply();

			return new SourceInfo(Texture, names[index], rectsPack.usedRectangles[index]);
		}

		#endregion

		#region File

		public static FileInfo Save(DynamicAtlas atlas, FileInfo info = null) {
			info ??= new FileInfo(atlas.Texture.name);
			if (Directory.Exists(info.Path) == false) Directory.CreateDirectory(info.Path);
			if (atlas.IsApplied == false) atlas.Apply();

			var bytes = atlas.Texture.EncodeToPNG();
			var json = JsonUtility.ToJson(atlas);

			File.WriteAllBytes(info.PathTexture, bytes);
			File.WriteAllText(info.PathData, json);

			return info;
		}

		public static DynamicAtlas Load(FileInfo info) {
			if (File.Exists(info.PathTexture) == false || File.Exists(info.PathData) == false)
				return null;

			var bytes = File.ReadAllBytes(info.PathTexture);
			var json = File.ReadAllText(info.PathData);

			var atlas = JsonUtility.FromJson<DynamicAtlas>(json);
			atlas.Texture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
			atlas.Texture.LoadImage(bytes);
			atlas.Texture.name = info.Name;

			return atlas;
		}

		public static bool Delete(FileInfo info) {
			if (File.Exists(info.PathTexture) == false && File.Exists(info.PathData) == false)
				return false;

			File.Delete(info.PathTexture);
			File.Delete(info.PathData);

			return true;
		}

		#endregion

		[Serializable]
		public class FileInfo {
			private static string DEFAULT_PATH = Application.persistentDataPath + "/DynamicAtlases/";
			private const string ExtensionTexture = ".png";
			private const string ExtensionData = ".json";

			[SerializeField] private string name;
			[SerializeField] private string path;

			public string Name => name;
			public string Path => path;
			public string PathTexture => Path + Name + ExtensionTexture;
			public string PathData => Path + Name + ExtensionData;

			public FileInfo(string name, string path = null) {
				this.name = name;
				this.path = (string.IsNullOrEmpty(path)) ? DEFAULT_PATH : path;
			}
		}

		public class SourceInfo {
			public string Name { get; private set; }
			public Rect Rect { get; private set; }

			private readonly Texture2D _Texture;

			public SourceInfo(Texture2D texture, string name, Rect rect) {
				_Texture = texture;
				Name = name;
				Rect = rect;
			}

			public Sprite GetSprite(Vector2 pilot) {
				var sprite = Sprite.Create(_Texture, Rect, pilot);
				sprite.name = Name;

				return sprite;
			}

			public Sprite GetSprite() {
				return GetSprite(new Vector2(0.5f, 0.5f));
			}
		}
	}
}