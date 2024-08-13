using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.Collider {
	[AddComponentMenu("Mesh Combiner/MC Collider Generator")]
	public class ColliderGenerator : MonoBehaviour {
		public AbstractMeshCombiner combiner;

		private void Awake() => Init();

		public void Init() {
			if (combiner is IRenderListener listener) listener.OnRenderersUpdated += UpdateColliders;
		}

		private void UpdateColliders(Renderer[] renderers) {
			renderers.ForEach(
				ren => {
					// We might not want to add colliders to skinned mesh renderers
					if (ren is not MeshRenderer) return;

					if (!ren.TryGetComponent(out MeshCollider col)) {
						col = ren.gameObject.AddComponent<MeshCollider>();
					}

					col.sharedMesh = ren.GetComponent<MeshFilter>().sharedMesh;
				}
			);
		}
	}
}