using System;
using TeoGames.Mesh_Combiner.Scripts.Combine;
using TeoGames.Mesh_Combiner.Scripts.Editor.MenuItems;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TeoGames.Mesh_Combiner.Scripts.Editor {
	[CustomEditor(typeof(AbstractCombinable), true)]
	[CanEditMultipleObjects]
	public class CombinableEditor : UnityEditor.Editor {
		private bool _ShowLiveSync;

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			targets.ForEach(RenderInstance);
		}

		private void RenderInstance(Object obj) {
			var comb = (AbstractCombinable)obj;

			try {
				Utils.ClearCache(comb);
				RenderBlendShapes(comb);
			} catch (Exception ex) {
				Debug.LogException(ex, comb);
			}
		}

		private void RenderBlendShapes(AbstractCombinable comb) {
			if (Selection.gameObjects.Length > 1) return;

			var cache = comb.GetCache();
			var mesh = cache.mesh;
			var isSkinnedMesh = cache.isSkinnedMesh;
			if (!isSkinnedMesh || !mesh) return;

			var blendShapeCount = mesh.blendShapeCount;
			var conf = comb.GetCache().blendShape;
			if (blendShapeCount > 0) {
				EditorGUILayout.Space();
				conf.enabled = Toggle(comb, "Bake Blend Shapes", conf.enabled);
				if (conf.enabled) {
					conf.merge = Toggle(comb, "Merge Blend Shapes", conf.merge);

					_ShowLiveSync = EditorGUILayout.Foldout(_ShowLiveSync, "Real Time Sync", true);
					if (_ShowLiveSync) {
						for (var i = 0; i < blendShapeCount; i++) {
							var shapeName = mesh.GetBlendShapeName(i);
							var isEnabled = conf.liveSync.Contains(shapeName);
							var state = Toggle(comb, "    " + mesh.GetBlendShapeName(i), isEnabled);
							if (state != isEnabled) {
								if (state) conf.liveSync.Add(shapeName);
								else conf.liveSync.Remove(shapeName);
								EditorUtility.SetDirty(comb);
							}
						}
					}
				}
			} else {
				conf.enabled = false;
			}
		}

		public static bool Toggle(Object obj, string label, bool curVal) {
			var res = EditorGUILayout.Toggle(label, curVal);
			if (res != curVal) EditorUtility.SetDirty(obj);

			return res;
		}
	}
}