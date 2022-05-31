
using UnityEngine;

public class BattleTile : PointerHandler
{
    Animator animator;
    public int state = 0;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        SetState(state);
    }

    public void SetState(int state)
    {
        this.state = state;
        animator.SetInteger("state", state);
    }
}
