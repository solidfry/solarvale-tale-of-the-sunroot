using System;
using System.Collections.Generic;

namespace TeoGames.Mesh_Combiner.Scripts.BlendShape {
	[Serializable]
	public class BlendShapeConfiguration {
		public bool enabled;
		public bool merge;
		public List<string> liveSync;
	}
}