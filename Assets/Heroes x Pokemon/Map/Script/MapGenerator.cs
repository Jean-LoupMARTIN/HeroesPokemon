
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MapGenerator : MonoBehaviour
{
    public bool gizmoUpdate = false;

    [HideInInspector]
    public int[,] map;

    // map generation
    public int w = 50, h = 50;
    public float noise = 0.1f;
    public Vector2 noiseOffset;
    public enum Shape { Flat, Cone, ConeInv }
    public Shape shape;
    public AnimationCurve shapeCurve;
    [Range(0, 1)]
    public float groundStart = 0.3f, groundEnd = 0.7f;
    public int resolution = 1;
    public Material waterMaterial, groundMaterial, blockMaterial;
    public float waterHeightStart = -1f, waterHeightEnd = -0.5f;
    public float groundHeightStart = -0.5f, groundHeightEnd = 0;
    public float blockHeightStart = -0.5f, blockHeightEnd = 0.5f;

    // display
    Transform waterLayer, groundLayer, blockLayer;

    
    void OnDrawGizmos()
    {
        if (gizmoUpdate)
        {
            gizmoUpdate = false;
            Awake();
        }
    }

    void Awake()
    {
        waterLayer  = transform.Find("Water");
        groundLayer = transform.Find("Ground");
        blockLayer  = transform.Find("Block");

        GenerateMap();
        GenerateMesh();
        UpdateNavMesh();
    }



    void GenerateMap()
    {
        // init map
        map = new int[w, h];
        Vector2 center = new Vector2((float)(w-1)/2, (float)(h-1)/2);
        float radius  = (new Vector2((float)(Mathf.Max(w, h)-1)/2, 0) - center).magnitude;

        for (int x = 0; x < w; x++) {
            for (int y = 0; y < h; y++)
            {
                int xn = x - x % resolution;
                int yn = y - y % resolution;
                float height = Mathf.PerlinNoise(xn * noise + noiseOffset.x, yn * noise + noiseOffset.y);
                float shape = 1;
                if      (this.shape == Shape.Cone)      shape = Mathf.Clamp01(1-(new Vector2(xn, yn) - center).magnitude / radius);
                else if (this.shape == Shape.ConeInv)   shape = Mathf.Clamp01(  (new Vector2(xn, yn) - center).magnitude / radius);
                shape = shapeCurve.Evaluate(shape);
                height *= shape;

                if      (height < groundStart)  map[x, y] = -1;
                else if (height > groundEnd)    map[x, y] = 1;
                else                            map[x, y] = 0;
            }
        }
    }

    void GenerateMesh()
    {
        GenerateLayer(-1, waterHeightStart,   waterHeightEnd,   waterLayer,  waterMaterial);
        GenerateLayer(0,  groundHeightStart,  groundHeightEnd,  groundLayer, groundMaterial);
        GenerateLayer(1,  blockHeightStart,   blockHeightEnd,   blockLayer,  blockMaterial);
    }

    void GenerateLayer(int layer, float heightStart, float heightEnd, Transform layerTransform, Material material)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int x = 0; x < w; x++) {
            for (int y = 0; y < h; y++) {
                if (map[x, y] == layer)
                {
                    Tool.AddSquare(
                        new Vector3(x,   heightEnd, y),
                        new Vector3(x,   heightEnd, y+1),
                        new Vector3(x+1, heightEnd, y+1),
                        new Vector3(x+1, heightEnd, y),
                        vertices,
                        triangles);

                    if (!(x-1 >= 0 && map[x-1, y] == layer))
                        Tool.AddSquare(
                            new Vector3(x, heightEnd,   y),
                            new Vector3(x, heightStart, y),
                            new Vector3(x, heightStart, y+1),
                            new Vector3(x, heightEnd,   y+1),
                            vertices,
                            triangles);

                    if (!(x+1 < w && map[x+1, y] == layer))
                        Tool.AddSquare(
                            new Vector3(x+1, heightEnd,   y),
                            new Vector3(x+1, heightEnd,   y+1),
                            new Vector3(x+1, heightStart, y+1),
                            new Vector3(x+1, heightStart, y),
                            vertices,
                            triangles);

                    if (!(y-1 >= 0 && map[x, y-1] == layer))
                        Tool.AddSquare(
                            new Vector3(x,   heightEnd,   y),
                            new Vector3(x+1, heightEnd,   y),
                            new Vector3(x+1, heightStart, y),
                            new Vector3(x,   heightStart, y),
                            vertices,
                            triangles);

                    if (!(y+1 < h && map[x, y+1] == layer))
                        Tool.AddSquare(
                            new Vector3(x,   heightEnd,   y+1),
                            new Vector3(x,   heightStart, y+1),
                            new Vector3(x+1, heightStart, y+1),
                            new Vector3(x+1, heightEnd,   y+1),
                            vertices,
                            triangles);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        layerTransform.GetComponent<MeshFilter>().mesh = mesh;
        if (layerTransform.TryGetComponent(out MeshCollider c)) c.sharedMesh = mesh;

        layerTransform.GetComponent<MeshRenderer>().material = material;
    }

    void UpdateNavMesh()
    {
        foreach (NavMeshSurface surface in GetComponents<NavMeshSurface>())
            surface.BuildNavMesh();
    } 

}
