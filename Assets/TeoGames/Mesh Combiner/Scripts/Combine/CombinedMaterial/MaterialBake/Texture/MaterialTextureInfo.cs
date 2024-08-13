using System;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial.MaterialBake.Texture {
	[Serializable]
	public class MaterialTextureInfo {
		[Tooltip("Property name of texture in output material")]
		public string textureName;

		[Tooltip("Property name in source material")]
		public string shaderPropertyName;
		
		[Tooltip("Optional. Used as alternative when texture is not provided or missing")]
		public string colorPropertyName;
	}
}