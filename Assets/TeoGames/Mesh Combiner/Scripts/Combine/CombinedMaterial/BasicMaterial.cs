using System;
using System.Collections.Generic;
using TeoGames.Mesh_Combiner.Scripts.BlendShape;
using TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial.MaterialBake;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using TeoGames.Mesh_Combiner.Scripts.Profile;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial {
	[Serializable]
	public class BasicMaterial {
		public Material material;
		public ShadowCastingMode shadow;
		public bool isStatic = true;
		public bool hasBlendShapes;

		public readonly Dictionary<int, AdvancedCombineInstance> Meshes =
			new Dictionary<int, AdvancedCombineInstance>();

		public CombineInstance Mesh { get; protected set; }

		public Transform[] Bones { get; protected set; } = Array.Empty<Transform>();

		public BlendShapeContainer blendShape = new BlendShapeContainer();

		public int LastUpdatedAt { get; set; } = Timer.MS();

		public BasicMaterial(Material mat, ShadowCastingMode shadow) {
			this.shadow = shadow;
			material = mat;
		}

		public void Updated() {
			LastUpdatedAt = Timer.MS();
		}

		public virtual void SetMesh(
			int cID,
			MeshParser parser,
			Transform[] bones,
			int subMeshIndex,
			Mesh mesh,
			bool forceMeshReset,
			bool isStaticMesh,
			Matrix4x4 rootMat,
			Transform renTransform,
			BlendShapeConfiguration blendShapeConf
		) {
			if (parser != null) mesh = parser.GetParsedMesh(mesh, forceMeshReset);

			Meshes[cID * 100 + subMeshIndex] = new AdvancedCombineInstance {
				Combine = new CombineInstance {
					transform = rootMat * renTransform.localToWorldMatrix, subMeshIndex = subMeshIndex, mesh = mesh,
				},
			};

			Updated();
		}

		public virtual void Build() {
			Clear();

			var mesh = new Mesh { indexFormat = IndexFormat.UInt32 };
#if MC_DEBUG
			var matName = material ? material.name : "null";
			mesh.name = $"[MAT] {matName}";
#endif

			var cnt = Meshes.Count;
			var meshes = new CombineInstance[cnt];
			var i = 0;
			if (Application.isPlaying) {
				foreach (var pair in Meshes) meshes[i++] = pair.Value.Combine;
			} else {
				foreach (var pair in Meshes) {
					var res = pair.Value.Combine;
					res.realtimeLightmapScaleOffset = res.lightmapScaleOffset = MeshExtension.PlaceCubesInCube(cnt, i, .01f);
					meshes[i++] = res;
				}
			}

			mesh.CombineMeshes(meshes, true, true, !Application.isPlaying);
			Mesh = new CombineInstance { mesh = mesh, subMeshIndex = 0 };
		}

		public virtual void Clear() {
			if (Mesh.mesh) Object.DestroyImmediate(Mesh.mesh, true);
			Updated();
		}
	}
}