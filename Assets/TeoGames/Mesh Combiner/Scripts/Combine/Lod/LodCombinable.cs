using System.Linq;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.Lod {
	[RequireComponent(typeof(Renderer))]
	[AddComponentMenu("Mesh Combiner/MC LOD Combinable")]
	public class LodCombinable : Combinable {
		public LODGroup Group { get; protected set; }
		public int Level { get; protected set; }
		public LOD[] Lods => Group.GetLODs();

		public override void Include() {
			var group = GetGroup();
			var obj = GetCombiner();
			if (obj && group) obj.Lod.Combiner.Include(this);
		}

		public override void Exclude() {
			var group = GetGroup();
			var obj = GetCombiner();
			if (obj && group) obj.Lod.Combiner.Exclude(this);
		}

		public LODGroup GetGroup() {
			if (!Group) {
				Group = GetComponentInParent<LODGroup>();
				var lods = Lods;
				for (var i = 0; i < lods.Length; i++) {
					var lod = lods[i];
					if (lod.renderers.Contains(cache.renderer)) {
						Level = i;
						break;
					}
				}
			}

			return Group;
		}
	}
}