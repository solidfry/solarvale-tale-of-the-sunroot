using System.Collections;
using System.Linq;
using TeoGames.Mesh_Combiner.Scripts.Combine;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Demo.Scripts {
	[AddComponentMenu("Mesh Combiner/Demo/MC Randomize Mesh")]
	[RequireComponent(typeof(MeshFilter))]
	public class RandomizeMesh : MonoBehaviour {
		[SerializeField] private float minDelay = .1f;
		[SerializeField] private float maxDelay = .5f;
		[SerializeField] private Mesh[] meshes;

		private MeshFilter Filter => GetComponent<MeshFilter>();
		private Combinable Combinable => GetComponent<Combinable>();

		private void Start() {
			StartCoroutine(ToggleMesh());
		}

		private IEnumerator ToggleMesh() {
			var delay = new WaitForSeconds(Random.Range(minDelay, maxDelay));
			var mesh = Filter.mesh = Instantiate(meshes[Random.Range(0, meshes.Length - 1)]);
			var sets = Enumerable
				.Range(0, 5)
				.Select(_ => mesh.vertices.Select(Randomize).ToArray())
				.ToArray();
			Combinable.ClearCache(true);

			while (this) {
				yield return delay;

				Combinable.GetCache().mesh.vertices = sets[Random.Range(0, sets.Length - 1)];
				Combinable.UpdateMesh();
			}
		}

		private static Vector3 Randomize(Vector3 orig) => orig + new Vector3(
			Random.Range(-.05f, 0.05f),
			Random.Range(-.05f, 0.05f),
			Random.Range(-.05f, 0.05f)
		);
	}
}