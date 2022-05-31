using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Mob : PointerHandler
{
    public string mobName = "Name";
    public int lvl=1;
    [HideInInspector]
    public float xp = 0, xpLvlUp = 1000;

    public float lifeMax = 50;
    [HideInInspector]
    public float life;
    public int speed = 5;
    public int initiative = 10;

    public float speedMoving = 1;
    public bool big = false;
    public bool fly = false; 
    public Texture2D icon;

    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public Outline outline;

    [HideInInspector]
    public Collider selectCollider;

    public Transform ragdollRoot;

    [HideInInspector]
    public UnityEvent lifeChangedEvent, xpChangedEvent, lvlChangedEvent, focusEvent, unfocusEvent;
    public void SetLife(float life) { this.life = life; lifeChangedEvent.Invoke(); }
    public void SetLvl(int lvl) { this.lvl = lvl; lvlChangedEvent.Invoke(); }

    [HideInInspector]
    public int x, y; // battleMap


    private void Awake()
    {
        animator        = GetComponent<Animator>();
        outline         = GetComponent<Outline>();
        selectCollider  = GetComponent<Collider>();
        outline.enabled = false;
        selectCollider.enabled = false;

        life = lifeMax;
        ActiveRaddoll(false);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) animator.SetTrigger("Attack");
        if (Input.GetKeyDown(KeyCode.X)) animator.SetTrigger("Shoot");
        if (Input.GetKeyDown(KeyCode.C)) animator.SetTrigger("Cast");
        if (Input.GetKeyDown(KeyCode.V)) TakeDamage(10, Vector3.forward);
        if (Input.GetKeyDown(KeyCode.B)) GainXp(300);
    }



    public void Focus(bool b)
    {
        if (b) {
            outline.OutlineWidth = 3;
            outline.OutlineColor = Color.white;
            outline.OutlineMode = Outline.Mode.OutlineAll;
            focusEvent.Invoke();
        }
        else {
            outline.OutlineWidth = 2;
            outline.OutlineColor = new Color(1, 1, 1, 0.2f);
            outline.OutlineMode = Outline.Mode.OutlineHidden;
            unfocusEvent.Invoke();
        }
    }



    public void ActiveRaddoll(bool active)
    {
        foreach (Collider c in ragdollRoot.GetComponentsInChildren<Collider>())
            c.enabled = active;

        foreach (Rigidbody rb in ragdollRoot.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = !active;
    }


    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        SetLife(life - damage);

        if (life <= 0)
        {
            animator.enabled = false;
            outline.enabled = false;
            ActiveRaddoll(true);

            foreach (Rigidbody rb in transform.GetComponentsInChildren<Rigidbody>())
                rb.AddForce(hitDirection.normalized * damage / lifeMax * 5000);
        }
        else animator.SetTrigger("TakeDamage");
    }



    public void GainXp(float dxp)
    {
        xp += dxp;

        while (xp >= xpLvlUp)
        {
            SetLvl(lvl+1);
            xp -= xpLvlUp;
        }

        xpChangedEvent.Invoke();
    }




    //IEnumerator MoveTo(Vector3 target)
    //{
    //    animator.SetBool("Moving", true);

    //    while (Tool.Dist(this, target) > speed * Time.deltaTime)
    //    {
    //        transform.rotation = Quaternion.LookRotation(target - transform.position);
    //        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    //        yield return new WaitForEndOfFrame();
    //    }

    //    transform.rotation = Quaternion.LookRotation(target - transform.position);
    //    transform.position = target;

    //    animator.SetBool("Moving", false);
    //}
}
