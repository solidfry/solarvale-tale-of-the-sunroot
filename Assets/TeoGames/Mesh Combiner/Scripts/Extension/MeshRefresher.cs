using TeoGames.Mesh_Combiner.Scripts.Combine;
using UnityEngine;
#if NET_4_6 || NET_STANDARD_2_0
#endif
#if UNITY_EDITOR
#endif

namespace TeoGames.Mesh_Combiner.Scripts.Extension {
	public static class MeshRefresherExtension {
		public static void SetSharedMesh(this MeshFilter filter, Mesh mesh) {
			filter.sharedMesh = mesh;
			if (filter.TryGetComponent(out AbstractCombinable comb)) comb.UpdateMesh();
		}

		public static void SetSharedMesh(this SkinnedMeshRenderer renderer, Mesh mesh) {
			renderer.sharedMesh = mesh;
			if (renderer.TryGetComponent(out AbstractCombinable comb)) comb.UpdateMesh();
		}

		public static void SetMesh(this MeshFilter filter, Mesh mesh) {
			filter.mesh = mesh;
			if (filter.TryGetComponent(out AbstractCombinable comb)) comb.UpdateMesh();
		}


		public static void SetSharedMaterial(this Renderer renderer, Material[] materials) {
			renderer.sharedMaterials = materials;
			if (renderer.TryGetComponent(out AbstractCombinable comb)) comb.UpdateMaterial();
		}

		public static void SetMaterial(this Renderer renderer, Material[] materials) {
			renderer.materials = materials;
			if (renderer.TryGetComponent(out AbstractCombinable comb)) comb.UpdateMaterial();
		}
	}
}