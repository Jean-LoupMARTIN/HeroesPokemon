using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    static public Cam inst;

    public float xRot = 50;
    public float dCenterCoef = 0.5f;
    public float distCoef = 0.75f;
    public float time = 0.8f;
    public AnimationCurve curve;

    [HideInInspector]
    public bool firstSetPos = true;

    private void Awake()
    {
        inst = this;
    }



    void SetPosInstante(Vector3 center, float dCenter, float xRot, float yRot, float dist)
    {
        transform.rotation = Quaternion.Euler(xRot, yRot, 0);
        transform.position = center;
        Vector3 forwardProj = transform.forward;
        forwardProj.y = 0;
        forwardProj.Normalize();
        transform.position -= forwardProj * dCenter;
        transform.position -= transform.forward * dist;
    }

    void SetPosInstante(int[,] map, float yRot)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);
        Vector3 center = new Vector3((float)w / 2, 0, (float)h / 2);

        float yRotRad = yRot * Mathf.Deg2Rad;
        float dCenter = w * h / Mathf.Sqrt(Mathf.Pow(Mathf.Sin(yRotRad), 2) * Mathf.Pow(h, 2) + Mathf.Pow(Mathf.Cos(yRotRad), 2) * Mathf.Pow(w, 2));
        dCenter *= dCenterCoef;

        SetPosInstante(center, dCenter, xRot, yRot, Mathf.Max(w, h) * distCoef);
    }

    public void SetPos(int[,] map, float yRot)
    {
        if (firstSetPos)
        {
            firstSetPos = false;
            SetPosInstante(map, yRot);
        }

        else StartCoroutine(SetPotTransi(map, yRot));
    }

    IEnumerator SetPotTransi(int[,] map, float yRot)
    {
        float t = 0;
        float yRotStart = transform.eulerAngles.y;
        float dyRot = yRot - yRotStart;
        if      (dyRot >   180) yRotStart += 360;
        else if (dyRot <= -180) yRotStart -= 360;

        while (t < time)
        {
            t += Time.deltaTime;
            SetPosInstante(map, Mathf.Lerp(yRotStart, yRot, curve.Evaluate(t/time)));
            yield return new WaitForEndOfFrame();
        }

        SetPosInstante(map, yRot);
    }
}
