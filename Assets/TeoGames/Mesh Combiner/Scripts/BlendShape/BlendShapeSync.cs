using System;
using System.Collections.Generic;
using System.Linq;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.BlendShape {
	[Serializable]
	public class BlendShapeSyncData {
		public int index;
		public SkinnedMeshRenderer renderer;
	}

	[AddComponentMenu("Mesh Combiner/Utils/MC Blend Shape Sync")]
	[RequireComponent(typeof(SkinnedMeshRenderer))]
	public class BlendShapeSync : MonoBehaviour {
		public List<BlendShapeSyncData> map;
		public List<BlendShapeSyncData> Map => map;

		private SkinnedMeshRenderer _Renderer;

		private void Awake() {
			_Renderer = GetComponent<SkinnedMeshRenderer>();
		}

		public void SetMap(Dictionary<string, (int, Transform)> newMap) {
			map = newMap.Values
				.Select(pair => new BlendShapeSyncData {
					index = pair.Item1,
					renderer = pair.Item2.GetComponent<SkinnedMeshRenderer>()
				})
				.ToList();
			enabled = map.Count > 0;
		}

		private void FixedUpdate() {
			if (map == null || map.Count == 0 || !_Renderer.sharedMesh) return;

			var len = _Renderer.sharedMesh.blendShapeCount;
			map.ForEach((data, i) => {
				if (i >= len || !data.renderer) return;

				_Renderer.SetBlendShapeWeight(i, data.renderer.GetBlendShapeWeight(data.index));
			});
		}
	}
}