using System;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial.MaterialBake.Texture {
	[Serializable]
	public class TextureInfo : NoTexture.TextureInfo {
		[Tooltip("Texture resize will be based on that texture if it's true")]
		public bool isDiffuseTexture;

		public FilterMode filterMode = FilterMode.Point;
		public string[] keywords;
	}
}