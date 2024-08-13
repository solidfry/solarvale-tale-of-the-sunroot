#if NET_4_6 || NET_STANDARD_2_0
#endif
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeoGames.Mesh_Combiner.Scripts.Extension {
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public static class TextureExtension {
		public static void MarkAsReadable(this Texture2D tex) => tex.SetReadable(true);

		public static void UnmarkAsReadable(this Texture2D tex) => tex.SetReadable(false);

		public static void SetReadable(this Texture2D tex, bool value) {
#if UNITY_EDITOR
			if (!tex || tex.isReadable == value) return;

			var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
            if (!importer) return;
			importer.isReadable = value;
			EditorUtility.SetDirty(importer);
			importer.SaveAndReimport();
#endif
		}
	}
}