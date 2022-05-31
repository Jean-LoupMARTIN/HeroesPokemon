using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SkillArc : MonoBehaviour
{
    public float radiusStart = 0.5f, radiusEnd = 1;
    public float angle = 30;
    public Color colorStart, colorEnd;

    public Transform skillsContainer, lvl1, lvl2, lvl3;


    private void OnDrawGizmos()
    {
        UpdateMesh();
        if (transform.parent) skillsContainer.rotation = transform.parent.rotation;
    }


    private void UpdateMesh()
    {
        int res = 10;
        Mesh mesh = new Mesh();

        // vertices
        Vector3[] vertices = new Vector3[res * 4];

        for (int i = 0; i < res; i++)
        {
            Quaternion rot = Quaternion.Euler(Vector3.forward * angle * i / (res - 1));
            vertices[i + res * 0] = rot * Vector3.up * Mathf.Lerp(radiusStart, radiusEnd, 1);
            vertices[i + res * 1] = rot * Vector3.up * Mathf.Lerp(radiusStart, radiusEnd, 0.66f);
            vertices[i + res * 2] = rot * Vector3.up * Mathf.Lerp(radiusStart, radiusEnd, 0.33f);
            vertices[i + res * 3] = rot * Vector3.up * Mathf.Lerp(radiusStart, radiusEnd, 0);
        }


        // triangles
        int[] t1 = new int[(res - 1) * 6];
        int[] t2 = new int[t1.Length];
        int[] t3 = new int[t1.Length];

        for (int i = 0; i < res-1; i++)
        {
            t1[i * 6 + 0] = i;
            t1[i * 6 + 1] = i + 1;
            t1[i * 6 + 2] = i + 1 + res;
            t1[i * 6 + 3] = i;
            t1[i * 6 + 4] = i + 1 + res;
            t1[i * 6 + 5] = i + res;

            t2[i * 6 + 0] = i + res;
            t2[i * 6 + 1] = i + 1 + res;
            t2[i * 6 + 2] = i + 1 + res * 2;
            t2[i * 6 + 3] = i + res;
            t2[i * 6 + 4] = i + 1 + res * 2;
            t2[i * 6 + 5] = i + res * 2;

            t3[i * 6 + 0] = i + res * 2;
            t3[i * 6 + 1] = i + 1 + res * 2;
            t3[i * 6 + 2] = i + 1 + res * 3;
            t3[i * 6 + 3] = i + res * 2;
            t3[i * 6 + 4] = i + 1 + res * 3;
            t3[i * 6 + 5] = i + res * 3;
        }

        mesh.vertices = vertices;
        mesh.subMeshCount = 3;
        mesh.SetTriangles(t1, 0);
        mesh.SetTriangles(t2, 1);
        mesh.SetTriangles(t3, 2);
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.materials[0].color = Color.Lerp(colorStart, colorEnd, 1f);
        renderer.materials[1].color = Color.Lerp(colorStart, colorEnd, 0.5f);
        renderer.materials[2].color = Color.Lerp(colorStart, colorEnd, 0f);
    }
}
