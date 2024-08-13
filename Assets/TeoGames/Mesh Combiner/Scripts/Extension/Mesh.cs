using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if NET_4_6 || NET_STANDARD_2_0
using System;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TeoGames.Mesh_Combiner.Scripts.Extension {
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public static class MeshExtension {
		private static readonly Dictionary<Mesh, Mesh> ToStaticCache = new Dictionary<Mesh, Mesh>();
		private static readonly Dictionary<Mesh, Mesh> ToAnimatedCache = new Dictionary<Mesh, Mesh>();

#if UNITY_EDITOR
		static MeshExtension() => EditorApplication.playModeStateChanged += _ => ResetCache();

		[InitializeOnEnterPlayMode]
		public static void ResetCache() {
			ToStaticCache.Clear();
			ToAnimatedCache.Clear();
		}
#endif

		public static void Save(this Mesh mesh, string path, bool overrideFile) {
#if UNITY_EDITOR
			if (!overrideFile) {
				var i = 0;
				var fixedPath = path;
				while (File.Exists($"{fixedPath}.asset")) {
					fixedPath = $"{path} ({++i})";
				}

				path = fixedPath;
			}

			Debug.Log($"Save mesh {path}.asset");
			AssetDatabase.CreateAsset(mesh, $"{path}.asset");
			AssetDatabase.SaveAssets();
#endif
		}

		public static Vector4 PlaceCubesInCube(int count, int i, float padding) {
			var mX = Mathf.CeilToInt(Mathf.Sqrt(count));
			var mY = (count + mX - 1) / mX;
			var scaleX = 1f / mX;
			var scaleY = 1f / mY;
			var column = i % mX;
			var row = i / mX;
			var xPosition = column * scaleX + padding;
			var yPosition = row * scaleY + padding;

			return new Vector4(scaleX - padding * 2, scaleY - padding * 2, xPosition, yPosition);
		}

		private static string GetRelativePath(string relativeTo, string path) {
#if NET_4_6 || NET_STANDARD_2_0
			var uri = new Uri(relativeTo);
			var rel = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(path)).ToString()).Replace(
				Path.AltDirectorySeparatorChar,
				Path.DirectorySeparatorChar
			);
			if (rel.Contains(Path.DirectorySeparatorChar.ToString()) == false) {
				rel = $".{Path.DirectorySeparatorChar}{rel}";
			}

			return rel;
#else
			return Path.GetRelativePath(relativeTo, path);
#endif
		}

		public static void Delete(this Mesh mesh) {
#if UNITY_EDITOR
			var path = AssetDatabase.GetAssetPath(mesh);
			if (string.IsNullOrEmpty(path)) return;

			var nm = mesh.name;
			Debug.Log($"Delete mesh {path}");
			var dir = Path.GetDirectoryName(path);

			AssetDatabase.DeleteAsset(path);

			while (dir != null && !Directory.EnumerateFileSystemEntries(dir).Any()) {
				Debug.Log($"Delete folder {dir}");
				AssetDatabase.DeleteAsset(dir);
				dir = Directory.GetParent(dir)?.FullName;
			}

			AssetDatabase.SaveAssets();
#endif
		}

		public static Mesh ToAnimated(this Mesh mesh) {
			if (ToAnimatedCache.TryGetValue(mesh, out var existing)) return existing;

			return ToAnimatedCache[mesh] = mesh.bindposes.Length > 0 ? mesh : mesh.CopyMesh();
		}

		public static Mesh ToStatic(this Mesh mesh) {
			if (ToStaticCache.TryGetValue(mesh, out var existing)) return existing;

			// Parse if there is more than one bind poses
			return ToStaticCache[mesh] = mesh.bindposes.Length == 1 ? mesh : mesh.CopyMesh();
		}

		private static Mesh CopyMesh(this Mesh mesh) {
			var bones = Enumerable.Repeat((byte)1, mesh.vertexCount).ToNativeArray();
			var weight = new BoneWeight1 { boneIndex = 0, weight = 1 };
			var weights = Enumerable.Repeat(weight, mesh.vertexCount).ToNativeArray();

			var res = Object.Instantiate(mesh);
			res.bindposes = new[] { Matrix4x4.identity };
			res.SetBoneWeights(bones, weights);

			bones.Dispose();
			weights.Dispose();

			return res;
		}

		public static void MarkAsReadable(this Mesh mesh) => mesh.SetReadable(true);

		public static void UnmarkAsReadable(this Mesh mesh) => mesh.SetReadable(false);

		public static void SetReadable(this Mesh mesh, bool value) {
#if UNITY_EDITOR
			if (!mesh || mesh.isReadable == value) return;

			if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(mesh)) is not ModelImporter importer) {
				var type = value ? "readable" : "non-readable";
				Debug.LogError($"Unable to mark mesh '{mesh.name}' as {type}, try to do it manually", mesh);

				return;
			}

			if (importer.optimizeGameObjects) {
				Debug.LogError(
					$"Mesh '{mesh.name}' is has enabled option 'Optimize Game Objects'. It will lead to inability to bake mesh. Please disable that option. See docs: https://teogames.gitbook.io/dynamic-mesh-combiner/components/mc-combinable#optimize-game-objects",
					mesh
				);
			}

			importer.isReadable = value;
			EditorUtility.SetDirty(importer);
			importer.SaveAndReimport();
#endif
		}

		public static bool IsAnimated(this Mesh mesh) {
			return mesh.bindposes.Length > 0;
		}
	}
}