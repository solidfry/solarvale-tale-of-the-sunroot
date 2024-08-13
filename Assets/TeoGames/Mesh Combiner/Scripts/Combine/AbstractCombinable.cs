using System;
using System.Collections.Generic;
using TeoGames.Mesh_Combiner.Scripts.BlendShape;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using TeoGames.Mesh_Combiner.Scripts.Util;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	public abstract class AbstractCombinable : MonoBehaviour {
		[SerializeField, CombinableCacheProperty]
		protected CombinableCache cache = new CombinableCache();

		public abstract bool IsCompatible(AbstractMeshCombiner combiner);
		public abstract AbstractMeshCombiner GetCombiner();
		public abstract void Include();
		public abstract void Exclude();
		public abstract bool IsStatic { get; }
		public abstract bool IsActive { get; }

		[Obsolete("GetRenderer is deprecated, please use GetCache instead.")]
		public virtual CombinableCache GetRenderer() => GetCache();

		public virtual CombinableCache GetCache() => cache;

		[Obsolete("SetRenderers is deprecated, please use CleanCache instead.")]
		public virtual void SetRenderers(bool force = false) => ClearCache(force);

		public virtual void ClearCache(bool force = false) {
			var isCached = cache.status > CacheStatus.None;
			if (!isCached || force) {
				var oldBs = cache.blendShape;
				cache = new CombinableCache { transform = transform, status = CacheStatus.MeshUpdated };
				cache.isSkinnedMesh = TryGetComponent(out cache.skinnedMeshRenderer);

				if (cache.isSkinnedMesh) {
					cache.renderer = cache.skinnedMeshRenderer;
					cache.mesh = cache.skinnedMeshRenderer.sharedMesh;

					cache.blendShape = oldBs ?? new BlendShapeConfiguration();
					cache.blendShape.enabled = isCached ? oldBs.enabled : cache.mesh.blendShapeCount > 0;
					if (!isCached) {
						cache.blendShape.liveSync = new List<string>(
							cache.blendShape.enabled
								? cache.mesh.GetBlendShapeNames()
								: Array.Empty<string>()
						);
					}
				} else {
					cache.meshRenderer = GetComponent<MeshRenderer>();
					cache.renderer = cache.meshRenderer;
					cache.meshFilter = GetComponent<MeshFilter>();
					cache.mesh = cache.meshFilter.sharedMesh;
				}

				cache.mesh.MarkAsReadable();
				cache.materials = cache.renderer.sharedMaterials;

				PostCacheSet();
			}

			Optimize();
		}

		protected virtual void PostCacheSet() { }

		public void UpdateStatus() {
			cache.materials = cache.renderer.sharedMaterials;
			if (IsActive) Include();
			else Exclude();
		}

		public void UpdateMesh() {
			cache.status = CacheStatus.MeshUpdated;
			UpdateStatus();
		}

		public void UpdateMaterial() {
			cache.materials = cache.renderer.sharedMaterials;
			UpdateStatus();
		}

		private void Optimize() {
			if (cache.isSkinnedMesh && !cache.IsOptimized) {
				cache.IsOptimized = true;

#if UNITY_EDITOR
				if (!EditorApplication.isPlaying) {
					cache.Bones = cache.skinnedMeshRenderer.bones;
					return;
				}
#endif

				var bones = cache.skinnedMeshRenderer.bones;
				var hasBones = bones.Length > 0;
				(cache.mesh, cache.Bones) = cache.skinnedMeshRenderer.sharedMesh.Optimize(
					hasBones ? bones : new[] { transform },
					hasBones
				);
				cache.IsOptimized = true;
			}
		}
	}
}