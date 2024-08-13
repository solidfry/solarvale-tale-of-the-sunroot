using System.Linq;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Demo.Scripts {
	[AddComponentMenu("Mesh Combiner/Demo/MC Edit LODs")]
	public class EditLods : MonoBehaviour {
		private void Start() {
			var group = GetComponent<LODGroup>();
			var lods = group.GetLODs();
			var last = lods.Last();
			var test = new LOD();
			test.fadeTransitionWidth += last.fadeTransitionWidth - .01f;
			test.screenRelativeTransitionHeight += last.screenRelativeTransitionHeight - .01f;
			group.SetLODs(lods.Concat(new[] { test }).ToArray());
		}
	}
}