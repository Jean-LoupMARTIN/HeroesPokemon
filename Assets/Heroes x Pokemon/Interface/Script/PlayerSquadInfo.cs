using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSquadInfo : MobInfos
{
    static public PlayerSquadInfo inst;

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        SetMobs(PlayerSquad.inst.mobs, true);

        foreach (MobInfo info in array)
        {
            info.SetState(0);
            info.enterEvent.AddListener(() => info.SetState(1));
            info.exitEvent .AddListener(() => info.SetState(0));
        }
    }
}
