using System.Collections;
using System.Collections.Generic;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Demo.Scripts {
	[AddComponentMenu("Mesh Combiner/Demo/MC Spawn Objects Inside Circle")]
	public class SpawnObjectsInsideCircle : MonoBehaviour {
		public float size = 1f;
		public float spawnDelaySec = .1f;
		public int objectsToKeep = 1000;
		public int offset;
		public GameObject prefab;
		public bool useLazeDestroy = true;

		public void SetUseLazeDestroy(bool val) => useLazeDestroy = val;

		private void OnEnable() => StartCoroutine(LazyAddObjects());

		private void OnDisable() => StopAllCoroutines();

		private IEnumerator LazyAddObjects() {
			var objects = new List<GameObject>();

			while (Application.isPlaying) {
				var pos = Random.insideUnitCircle.ToVector3() * size;
				var scale = Random.Range(.6f, 1f) * Vector3Extensions.One;

				if (objects.Count >= objectsToKeep) {
					var first = objects[0];
					objects.RemoveAt(0);

					if (useLazeDestroy) {
						first.LazyDestroy();

						AddNew(pos, scale, objects);
					} else {
						first.transform.localPosition = pos;
						first.transform.localScale = scale;
						objects.Add(first);
					}
				} else AddNew(pos, scale, objects);

				yield return new WaitForSecondsRealtime(spawnDelaySec);
			}
		}

		private void AddNew(Vector3 pos, Vector3 scale, List<GameObject> objects) {
			var obj = Instantiate(prefab, transform);
			obj.transform.localPosition = pos;
			obj.transform.localScale = scale;
			obj.name = $"[{(offset + objects.Count):0000}] {prefab.name}";
			objects.Add(obj);
		}
	}
}