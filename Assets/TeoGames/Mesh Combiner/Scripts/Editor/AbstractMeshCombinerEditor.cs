using System.Linq;
using TeoGames.Mesh_Combiner.Scripts.Combine;
using TeoGames.Mesh_Combiner.Scripts.Combine.Collider;
using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using TeoGames.Mesh_Combiner.Scripts.Editor.MenuItems;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using TeoGames.Mesh_Combiner.Scripts.Extension.Editor;
using UnityEditor;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Editor {
	[CustomEditor(typeof(AbstractMeshCombiner), true)]
	[CanEditMultipleObjects]
	public class AbstractMeshCombinerEditor : UnityEditor.Editor {
		private static readonly int AssetExtensionLength = ".asset".Length;

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			var textStyle = EditorStyles.label;
			textStyle.wordWrap = true;

			targets.ForEach(
				t => {
					if (t is not ICombinerVisibilityTogglable comb) return;

					VisibilityToggler.Toggle(comb);
				}
			);

			targets.ForEach(
				t => {
					if (t is not ChunkMeshCombiner cmc) return;

					var status = cmc.updateQueue.IsStarted ? "active" : "inactive";
					GUILayout.Label($"Update queue is {status}, with {cmc.updateQueue.Count} items", textStyle);
				}
			);

			GUILayout.Space(20);

			if (targets.Length != 1) {
				GUILayout.Label("Mesh baking can work only with single selected combiner", textStyle);
			} else {
				var c = (AbstractMeshCombiner)target;
				var go = c.gameObject;

				GUILayout.Label(
					"Mesh baking will generate all renderers as it works in runtime and save meshes to the file, after baking all original meshes will be disabled.",
					textStyle
				);

				if (GUILayout.Button("Bake mesh")) {
					var isPrefab = go.scene.name == null;
					var (obj, folder) = isPrefab || go.scene.path == ""
						? go.OpenPrefab()
						: (go, go.scene.path.RemoveLast(AssetExtensionLength) + "/");

					BakingUtils.Bake(obj, folder).ContinueWith(
						() => {
							if (isPrefab) obj.SaveAndClosePrefab();
						}
					);
				}

				if (BakingUtils.IsBaked(go) && GUILayout.Button("Remove baked mesh")) {
					var obj = go.scene.name != null ? go : go.OpenPrefab().root;
					BakingUtils.RemoveBake(obj);
					obj.SaveAndClosePrefab();
				}

				if (c is IRenderListener) {
					GUILayout.Space(10);

					var col = go.GetComponents<ColliderGenerator>().FirstOrDefault(o => o.combiner == c);
					if (col) {
						if (GUILayout.Button("Remove mesh collider generator")) {
							DestroyImmediate(col, true);
							EditorUtility.SetDirty(go);
						}
					} else {
						if (GUILayout.Button("Add mesh collider generator")) {
							col = go.gameObject.AddComponent<ColliderGenerator>();
							col.combiner = c;
							EditorUtility.SetDirty(col);
						}
					}
				}
			}
		}
	}
}