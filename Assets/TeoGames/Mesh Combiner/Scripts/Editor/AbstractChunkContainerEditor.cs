using TeoGames.Mesh_Combiner.Scripts.Combine.ChunkContainer;
using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEditor;

namespace TeoGames.Mesh_Combiner.Scripts.Editor {
	[CustomEditor(typeof(AbstractChunkContainer), true)]
	[CanEditMultipleObjects]
	public class AbstractChunkContainerEditor : UnityEditor.Editor {
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
		}
	}
}