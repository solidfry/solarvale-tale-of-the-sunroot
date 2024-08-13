using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeoGames.Mesh_Combiner.Scripts.BlendShape;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using TeoGames.Mesh_Combiner.Scripts.Profile;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.MeshRendererManager {
	public class DynamicMeshRenderer : AbstractMeshRenderer {
		private readonly SkinnedMeshRenderer _Renderer;
		private readonly Transform _Parent;
		private readonly bool _BlendShapeEnabled;
		private readonly List<Transform> _Bones = new List<Transform>();
		private readonly BlendShapeContainer _BlendShape = new BlendShapeContainer();

		private BlendShapeSync _Sync;

		public override Renderer Renderer => _Renderer;

		public DynamicMeshRenderer(bool blendShapeEnabled, ShadowCastingMode shadow, GameObject obj) : base(shadow) {
			var bs = blendShapeEnabled ? "[BS] " : "";
			obj.name = $"{bs}[SMR] [S={shadow.ToString()}] {obj.name}";

			_Parent = obj.transform;

			_Renderer = obj.AddComponent<SkinnedMeshRenderer>();
			_Renderer.shadowCastingMode = shadow;

			// Weird fix that will increase SMR performance twice...
			_Renderer.updateWhenOffscreen = false;
			Task.CompletedTask.WaitForUpdate().ContinueWith(() => _Renderer.updateWhenOffscreen = true).Forget();

			_BlendShapeEnabled = blendShapeEnabled;
		}

		public override void Reset() {
			base.Reset();

			ProfilerModule.TotalBones.Value -= _Bones.Count;

			_BlendShape.Clear();
			_Bones.Clear();
		}

		public override void Clear() {
			base.Clear();

			ProfilerModule.TotalBones.Value -= _Bones.Count;
		}

		public override bool Validate(bool blendShapes, bool isStatic, ShadowCastingMode shadow) =>
			!isStatic && shadow == Shadow && _BlendShapeEnabled == blendShapes;

		public override async Task BuildMesh(Timer timer, bool clearCache) {
			if (!IsChanged) return;

			await base.BuildMesh(timer, false);

			_BlendShape.Clear();
			foreach (var m in Materials) {
				_Bones.AddRange(m.Bones);
				if (m.hasBlendShapes) _BlendShape.Extend(m.blendShape);
				else _BlendShape.length += m.Mesh.mesh.vertexCount;
			}

			await ApplyBlendShapes(timer);

			if (clearCache) Materials.ForEach(m => m.Clear());
			ProfilerModule.TotalBones.Value += _Bones.Count;
		}

		private async Task ApplyBlendShapes(Timer timer) {
			if (!_BlendShapeEnabled) return;

			if (!_Sync) _Sync = _Renderer.gameObject.AddComponent<BlendShapeSync>();

			var length = OutMesh.vertexCount;
			var vertices = new Vector3[length];
			var normals = new Vector3[length];
			var tangents = new Vector3[length];
			var weight = 0f;

			foreach (var shape in _BlendShape.blendShape) {
				if (timer.IsTimeoutRequired) await timer.Wait();

				foreach (var pair in shape.Value) {
					var frame = pair.Value;
					var pos = pair.Key;
					weight = frame.weight;

					Array.Copy(frame.vertices, 0, vertices, pos, frame.vertices.Length);
					Array.Copy(frame.normals, 0, normals, pos, frame.normals.Length);
					Array.Copy(frame.tangents, 0, tangents, pos, frame.tangents.Length);
				}

				OutMesh.AddBlendShapeFrame(shape.Key, weight, vertices, normals, tangents);

				foreach (var pair in shape.Value) {
					var frame = pair.Value;
					var pos = pair.Key;

					ZeroArray(vertices, pos, frame.vertices.Length);
					ZeroArray(normals, pos, frame.normals.Length);
					ZeroArray(tangents, pos, frame.tangents.Length);
				}
			}
		}

		private void ZeroArray(Vector3[] arr, int idx, int length) {
			var bound = idx + length;
			for (var i = idx; i < bound; i++) arr[i] = Vector3Extensions.Zero;
		}

		public override Renderer BuildRenderer() {
			if (!IsChanged) return Renderer;

			IsChanged = false;
			var mesh = _Renderer.sharedMesh;
			if (mesh) {
				mesh.Clear();
				Object.DestroyImmediate(mesh, true);
			}

			var isEnabled = Renderer.enabled = ShouldBeActive;
			if (_BlendShapeEnabled && _Sync) _Sync.enabled = isEnabled;
			if (!isEnabled) return null;

			ProfilerModule.MeshRenderers.Value++;

			_Renderer.rootBone = _Parent;
			_Renderer.sharedMesh = OutMesh;
			_Renderer.bones = _Bones.ToArray();
			_Renderer.materials = Materials.Convert(m => m.material);

			if (_BlendShapeEnabled && _Sync) _Sync.SetMap(_BlendShape.liveSync);

			return _Renderer;
		}
	}
}