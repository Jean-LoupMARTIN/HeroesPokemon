using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobInfo : PointerHandlerUI
{
    [HideInInspector]
    public Mob mob;

    public RawImage icon, background;
    public Bar lifebar, xpbar;
    public TMP_Text nameTxt, lvlTxt;
    Animator animator;
    public Color backgroundColor = Color.red;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }






    public void SetMob(Mob mob, bool displayXp)
    {
        if (this.mob)
        {
            this.mob.lvlChangedEvent .RemoveListener(UpdateLvl);
            this.mob.lifeChangedEvent.RemoveListener(UpdateLife);
            this.mob.xpChangedEvent  .RemoveListener(UpdateXp);
        }

        this.mob = mob;
        if (!mob) return;

        icon.texture = mob.icon;
        nameTxt.text = mob.mobName;
        background.color = backgroundColor;

        UpdateLvl();
        mob.lvlChangedEvent.AddListener(UpdateLvl);

        UpdateLife();
        mob.lifeChangedEvent.AddListener(UpdateLife);

        UpdateXp();
        mob.xpChangedEvent.AddListener(UpdateXp);
        xpbar.gameObject.SetActive(displayXp);
    }


    void UpdateLvl() => lvlTxt.text = "N." + mob.lvl;
    void UpdateLife() => lifebar.SetProgress(mob.life / mob.lifeMax);
    void UpdateXp() => xpbar.SetProgress(mob.xp / mob.xpLvlUp);



    public void SetState(int state) => animator.SetInteger("state", state);
}
