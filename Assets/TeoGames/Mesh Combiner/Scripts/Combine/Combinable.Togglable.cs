using System;
using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	public partial class Combinable : IVisibilityToglable {
		[Tooltip("Will disable renderer when combined")]
		public bool disableRenderer = true;

		// TODO Need to find a way to do it without affecting performance for the rest 99% of use cases
		// [Tooltip("Will disable collider when combined")]
		// public bool disableCollider;

		[Tooltip("Will remove all materials when combined")]
		public bool removeMaterials;

		private int _ActiveInclusions;

		public virtual void OnExclude() {
#if DEBUG_BAKING
			Debug.LogError($"{name} > Combinable > OnExclude {_ActiveInclusions}");
#endif
			if (_ActiveInclusions-- != 1 || !cache.renderer) return;
			if (disableRenderer) cache.renderer.Enable();
			// if (disableCollider) cache.collideer.Enable();
			if (removeMaterials) cache.renderer.sharedMaterials = cache.materials;
		}

		public virtual void OnInclude() {
#if DEBUG_BAKING
			Debug.LogError($"{name} > Combinable > OnInclude {_ActiveInclusions}");
#endif
			if (_ActiveInclusions++ > 0 || !cache.renderer) return;
			if (disableRenderer) cache.renderer.Disable();
			// if (disableCollider) cache.collideer.Disable();
			if (removeMaterials) cache.renderer.sharedMaterials = Array.Empty<Material>();
		}
	}
}