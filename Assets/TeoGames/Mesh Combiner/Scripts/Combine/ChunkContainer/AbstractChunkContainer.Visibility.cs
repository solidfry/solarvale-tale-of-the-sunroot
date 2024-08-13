using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.ChunkContainer {
	public abstract partial class AbstractChunkContainer : ICombinerVisibilityTogglable {
		protected abstract void OnVisibilityChanged();

		[SerializeField, HideInInspector] internal bool isVisible = true;

		public bool IsVisible
		{
			get => isVisible;
			set
			{
				if (isVisible == value) return;

				isVisible = value;
				if (Application.isPlaying) OnVisibilityChanged();
			}
		}
	}
}