using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobsOrderIcon : PointerHandlerUI
{
    public RawImage icon, background;

    public void SetMob(Mob mob, Color color)
    {
        icon.texture = mob.icon;
        background.color = color;

        enterEvent.RemoveAllListeners();
        exitEvent .RemoveAllListeners();

        enterEvent.AddListener(() => mob.Focus(true));
        exitEvent .AddListener(() => mob.Focus(false));
    }
}
