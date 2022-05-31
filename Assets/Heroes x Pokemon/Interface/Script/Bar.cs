using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    public RawImage bar;
    public Gradient gradient;
        
    public void SetProgress(float p)
    {
        p = Mathf.Clamp01(p);
        bar.transform.localScale = new Vector3(p,1,1);
        bar.color = gradient.Evaluate(p);
    }
}
