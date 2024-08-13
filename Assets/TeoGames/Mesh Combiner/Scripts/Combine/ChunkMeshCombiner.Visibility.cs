using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	public partial class ChunkMeshCombiner : ICombinerVisibilityTogglable {
		[SerializeField, HideInInspector] private bool isVisible = true;

		public bool IsVisible
		{
			get => isVisible;
			set
			{
				if (isVisible == value) return;

				isVisible = value;
				chunks.ForEach(c => c.IsVisible = value);
				if (Application.isPlaying) Lod.Combiner.chunk.IsVisible = value;
			}
		}
	}
}