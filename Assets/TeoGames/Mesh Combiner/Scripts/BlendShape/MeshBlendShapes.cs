using System.Collections.Generic;
using System.Linq;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TeoGames.Mesh_Combiner.Scripts.BlendShape {
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public static class MeshBlendShapesExtension {
		private static readonly Dictionary<long, BlendShapeContainer> Cache =
			new Dictionary<long, BlendShapeContainer>();

#if UNITY_EDITOR
		static MeshBlendShapesExtension() => EditorApplication.playModeStateChanged += _ => ResetCache();

		[InitializeOnEnterPlayMode]
		private static void ResetCache() => Cache.Clear();
#endif

		public static BlendShapeContainer GetBlendShape(
			this Mesh mesh,
			int subMeshIndex,
			BlendShapeConfiguration conf
		) {
			var meshKey = (long)mesh.GetInstanceID() * 100 + (conf.enabled ? 10 : 0);
			var key = meshKey + subMeshIndex;
			if (Cache.TryGetValue(key, out var res)) return res;

			var shapes = conf.enabled ? mesh.blendShapeCount : 0;
			var subMeshes = new (BlendShapeContainer, SubMeshDescriptor)[mesh.subMeshCount];
			for (var i = 0; i < mesh.subMeshCount; i++) {
				var subMesh = mesh.GetSubMesh(i);
				subMeshes[i] = (new BlendShapeContainer { length = subMesh.vertexCount }, subMesh);
			}

			for (var i = 0; i < shapes; i++) {
				var shapeName = mesh.GetBlendShapeName(i);
				var frameCount = mesh.GetBlendShapeFrameCount(i);

				for (var index = 0; index < frameCount; index++) {
					var deltaVertices = new Vector3[mesh.vertexCount];
					var deltaNormals = new Vector3[mesh.vertexCount];
					var deltaTangents = new Vector3[mesh.vertexCount];

					mesh.GetBlendShapeFrameVertices(i, index, deltaVertices, deltaNormals, deltaTangents);
					foreach (var row in subMeshes) {
						var (data, subMesh) = row;
						if (!data.blendShape.TryGetValue(shapeName, out var list)) {
							data.blendShape[shapeName] = list = new Dictionary<int, BlendShapeFrame>();
						}

						var start = subMesh.firstVertex;
						var count = data.length;
						list.Add(
							0,
							new BlendShapeFrame {
								weight = mesh.GetBlendShapeFrameWeight(i, index),
								vertices = deltaVertices.Take(start, count).ToArray(),
								normals = deltaNormals.Take(start, count).ToArray(),
								tangents = deltaTangents.Take(start, count).ToArray(),
							}
						);
					}
				}
			}

			for (var i = 0; i < subMeshes.Length; i++) {
				var (list, _) = subMeshes[i];

				Cache[meshKey + i] = list;
				if (i == subMeshIndex) res = list;
			}

			return res;
		}

		public static IEnumerable<string> GetBlendShapeNames(this Mesh mesh) {
			var shapes = mesh.blendShapeCount;
			for (var i = 0; i < shapes; i++) {
				yield return mesh.GetBlendShapeName(i);
			}
		}
	}
}