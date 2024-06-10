using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace FlatKit {
public static class MeshSmoother {
    private const int SmoothNormalUVChannel = 2;

    /// <summary>
    /// Performs normal smoothing on the current mesh filter associated with this component asynchronously.
    /// This method will not try and re-smooth meshes which have already been smoothed.
    /// </summary>
    /// <returns>A task which will complete once normal smoothing is finished.</returns>
    public static Task SmoothNormalsA(Mesh mesh) {
        // Create a copy of the vertices and normals and apply the smoothing in an async task.
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var asyncTask = Task.Run(() => CalculateSmoothNormals(vertices, normals));

        // Once the async task is complete, apply the smoothed normals to the mesh on the main thread.
        return asyncTask.ContinueWith(i => { mesh.SetUVs(SmoothNormalUVChannel, i.Result); },
            TaskScheduler.FromCurrentSynchronizationContext());
    }

    public static void SmoothNormals(Mesh mesh) {
        var result = CalculateSmoothNormals(mesh.vertices, mesh.normals);
        mesh.SetUVs(SmoothNormalUVChannel, result);
    }

    public static void ClearNormalsUV(Mesh mesh) {
        mesh.SetUVs(SmoothNormalUVChannel, (Vector3[])null);
    }

    public static bool HasSmoothNormals(Mesh mesh) {
        return mesh.uv3 != null && mesh.uv3.Length > 0;
    }

    /// <summary>
    /// This method groups vertices in a mesh that share the same location in space then averages the normals of those vertices.
    /// For example, if you imagine the 3 vertices that make up one corner of a cube. Normally there will be 3 normals facing in the direction 
    /// of each face that touches that corner. This method will take those 3 normals and average them into a normal that points in the 
    /// direction from the center of the cube to the corner of the cube.
    /// </summary>
    /// <param name="vertices">A list of vertices that represent a mesh.</param>
    /// <param name="normals">A list of normals that correspond to each vertex passed in via the vertices param.</param>
    /// <returns>A list of normals which are smoothed, or averaged, based on share vertex position.</returns>
    public static List<Vector3>
        CalculateSmoothNormals(IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector3> normals) {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        // Group all vertices that share the same location in space.
        var groupedVertices = new Dictionary<Vector3, List<KeyValuePair<int, Vector3>>>();

        for (int i = 0; i < vertices.Count; ++i) {
            var vertex = vertices[i];

            if (!groupedVertices.TryGetValue(vertex, out var group)) {
                group = new List<KeyValuePair<int, Vector3>>();
                groupedVertices[vertex] = group;
            }

            group.Add(new KeyValuePair<int, Vector3>(i, vertex));
        }

        var smoothNormals = new List<Vector3>(normals);

        // If we don't hit the degenerate case of each vertex is its own group (no vertices shared a location), average the normals of each group.
        if (groupedVertices.Count != vertices.Count) {
            foreach (var group in groupedVertices) {
                var smoothingGroup = group.Value;

                // No need to smooth a group of one.
                if (smoothingGroup.Count != 1) {
                    var smoothedNormal = smoothingGroup.Aggregate(Vector3.zero,
                        (current, vertex) => current + normals[vertex.Key]);
                    smoothedNormal.Normalize();

                    foreach (var vertex in smoothingGroup) {
                        smoothNormals[vertex.Key] = smoothedNormal;
                    }
                }
            }
        }

        Debug.Log($"<b>[Flat Kit]</b> Generated smooth normals for <i>{vertices.Count}</i> vertices in " +
                  $"<i>{watch.ElapsedMilliseconds}</i> ms.");

        return smoothNormals;
    }
}
}