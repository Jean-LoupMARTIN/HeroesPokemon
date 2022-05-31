using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MobInfos : MonoBehaviour
{
    public MobInfo mobInfoPrefab;
    public float dy = 100;

    [HideInInspector]
    public MobInfo[] array;

    public Color backgroundColor = Color.blue;


    public void SetMobs(List<Mob> mobs, bool displayXp = true)
    {
        Clear();

        if (mobs == null)
        {
            array = null;
            return;
        }

        array = new MobInfo[mobs.Count];

        for (int i = 0; i < mobs.Count; i++)
        {
            MobInfo mobInfo = Instantiate(mobInfoPrefab, Vector3.zero, Quaternion.identity, transform);
            mobInfo.transform.localPosition = Vector3.down * i * dy;
            mobInfo.backgroundColor = backgroundColor;
            mobInfo.SetMob(mobs[i], displayXp);
            array[i] = mobInfo;
        }
    }

    public void Clear()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
    }

    public void SetAllState(int state)
    {
        foreach (MobInfo info in array)
            info.SetState(state);
    }
}
