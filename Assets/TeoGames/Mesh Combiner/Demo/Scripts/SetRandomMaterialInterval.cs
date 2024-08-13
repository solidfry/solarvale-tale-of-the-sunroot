using System.Collections;
using TeoGames.Mesh_Combiner.Scripts.Combine;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Demo.Scripts {
	[AddComponentMenu("Mesh Combiner/Demo/MC Set random material interval")]
	[RequireComponent(typeof(AbstractCombinable))]
	public class SetRandomMaterialInterval : MonoBehaviour {
		public Material[] materials;
		public float interval = 1f;

		private void OnEnable() {
			StartCoroutine(RunSetMateriial());
		}

		private IEnumerator RunSetMateriial() {
			var ren = GetComponent<Renderer>();
			var awaiter = new WaitForSeconds(interval);

			while (enabled) {
				ren.SetSharedMaterial(materials.GetRandomElements(ren.sharedMaterials.Length));

				yield return awaiter;
			}
		}
	}
}