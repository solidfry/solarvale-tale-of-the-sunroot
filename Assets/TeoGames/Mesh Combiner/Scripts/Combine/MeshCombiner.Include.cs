using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TeoGames.Mesh_Combiner.Scripts.Combine.CombinedMaterial;
using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using TeoGames.Mesh_Combiner.Scripts.Combine.MaterialStorage;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using TeoGames.Mesh_Combiner.Scripts.Profile;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	public partial class MeshCombiner {
		private AbstractMaterialStorage _Materials;
		private Transform[] _StaticBones;
		private Func<AbstractCombinable, bool> _StaticInclude;
		private Func<AbstractCombinable, bool> _DynamicInclude;
		private Matrix4x4 _Matrix;

		[SuppressMessage("ReSharper", "RedundantCast")]
		private void DefineIncludes(bool supportStatic, bool supportDynamic) {
			_StaticBones = new[] { transform };
			_StaticInclude = supportStatic ? (Func<AbstractCombinable, bool>)IncludeAsStatic : IncludeAsDynamic;
			_DynamicInclude = supportDynamic ? (Func<AbstractCombinable, bool>)IncludeAsDynamic : IncludeAsStatic;
		}

		public override void Include(AbstractCombinable combinable) {
			_Updates[combinable] = UpdateType.Include;

#if DEBUG_BAKING
			Debug.LogError($">>>>> {combinable.name} > Include > {name} > {isVisible} > {IsUpdateInProgress} > {_Updates.Count}");
#endif

			if (IsVisible) ScheduleUpdate();
			else CallCombinableInclude(combinable);
		}

		private void CallCombinableInclude(AbstractCombinable comb) {
			if (comb is IVisibilityToglable t) CallCombinableInclude(t);
		}

		private void CallCombinableInclude(IVisibilityToglable togglable) {
			togglable.OnInclude();
			_ToggleUpdates[togglable] = ToggleStatus.Skip;
		}

		protected bool RunInclude(AbstractCombinable combinable) {
			if (!combinable.IsActive) return false;

			combinable.ClearCache();

			return combinable.IsStatic ? _StaticInclude(combinable) : _DynamicInclude(combinable);
		}

		private bool IncludeAsStatic(AbstractCombinable combinable) {
			var cache = combinable.GetCache();
			var mats = cache.materials;
			var isMeshUpdated = cache.status == CacheStatus.MeshUpdated;
			var subMeshes = Math.Min(mats.Length, cache.mesh.subMeshCount);
			var shadow = cache.renderer.shadowCastingMode;
			var offset = 1 + ((int)shadow + 1) * 10 + 0;
			var newMaterials = new BasicMaterial[subMeshes];
			var cID = combinable.GetInstanceID();
			var rTransform = cache.transform;

			if (isMeshUpdated) cache.status = CacheStatus.Cached;

			for (var i = 0; i < subMeshes; i++) {
				var mat = mats[i];
				var mID = mat.GetCombineID(offset);
				var (parser, material) = _Materials.Get(mID, mat, offset, shadow, true);
				newMaterials[i] = material;

				material.SetMesh(
					cID,
					parser,
					null,
					i,
					cache.mesh,
					isMeshUpdated,
					true,
					_Matrix,
					rTransform,
					null
				);
			}

			AddMaterialMap(combinable, cID, newMaterials);

			return true;
		}

		private bool IncludeAsDynamic(AbstractCombinable combinable) {
			var cache = combinable.GetCache();
			var mats = cache.materials;
			var isMeshUpdated = cache.status == CacheStatus.MeshUpdated;
			var subMeshes = Math.Min(mats.Length, cache.mesh.subMeshCount);
			var shadow = cache.renderer.shadowCastingMode;
			var blendShape = cache.blendShape;
			var offset = ((int)shadow + 1) * 10 + (separateBlendShapes && blendShape.enabled ? 100 : 0);
			var newMaterials = new BasicMaterial[subMeshes];
			var cID = combinable.GetInstanceID();
			var isStatic = combinable.IsStatic;

			Mesh parsedMesh;
			if (isMeshUpdated) {
				parsedMesh = cache.mesh = cache.isCorrectionRequired switch {
					MeshCorrection.Stat => cache.mesh.ToStatic(),
					MeshCorrection.Anim => cache.mesh.ToAnimated(),
					_ => cache.mesh
				};
				cache.status = CacheStatus.Cached;
			} else {
				parsedMesh = cache.mesh;
			}

			var rTransform = cache.transform;
			var realBones = isStatic ? _StaticBones : cache.Bones ?? new[] { rTransform };

			for (var i = 0; i < subMeshes; i++) {
				var mat = mats[i];
				var mID = mat.GetCombineID(offset);
				var (parser, material) = _Materials.Get(mID, mat, offset, shadow, false);
				newMaterials[i] = material;

				material.SetMesh(
					cID,
					parser,
					realBones,
					i,
					parsedMesh,
					isMeshUpdated,
					isStatic,
					_Matrix,
					rTransform,
					blendShape
				);
			}

			AddMaterialMap(combinable, cID, newMaterials);

			return true;
		}

		private void AddMaterialMap(AbstractCombinable combinable, int cID, BasicMaterial[] newMaterials) {
			if (_CombinableToMaterial.TryGetValue(combinable, out var existing)) {
				for (var i = 0; i < existing.materials.Length; i++) {
					var material = existing.materials[i];
					if (newMaterials.Contains(material)) continue;

					var meshKey = existing.cid * 100 + i;
					if (!material.Meshes.ContainsKey(meshKey)) continue;

					material.Meshes.Remove(meshKey);
					material.Updated();
				}
			}

			_CombinableToMaterial[combinable] = (cID, newMaterials);
			ProfilerModule.Meshes.Value++;
		}
	}
}