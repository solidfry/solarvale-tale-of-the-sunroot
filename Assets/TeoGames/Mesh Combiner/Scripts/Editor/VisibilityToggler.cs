using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Editor {
	public abstract class VisibilityToggler {
		public static void Toggle(ICombinerVisibilityTogglable comb) {
			comb.IsVisible = CombinableEditor.Toggle(comb as Object, "Is Visible", comb.IsVisible);
		}
	}
}