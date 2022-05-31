using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Squad : MonoBehaviour
{
    [HideInInspector]
    public List<Mob> mobs;

    [HideInInspector]
    public Mob head;

    [HideInInspector]
    public NavMeshAgent agent;

    protected virtual void Awake()
    {
        mobs = GetComponentsInChildren<Mob>().ToList();
        head = mobs[0];

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        string agentTypeName;
        if      ( head.big &&  head.fly) agentTypeName = "2x2 fly";
        else if ( head.big && !head.fly) agentTypeName = "2x2";
        else if (!head.big &&  head.fly) agentTypeName = "1x1 fly";
        else                             agentTypeName = "1x1";
        agent.agentTypeID = Tool.GetAgentIDByName(agentTypeName);
    }

    protected virtual void Start()
    {
        for (int i = 0; i < mobs.Count; i++)
        {
            mobs[i].transform.localPosition = Vector3.zero;
            mobs[i].transform.localRotation = Quaternion.identity;
            mobs[i].gameObject.SetActive(i == 0);
        }
    }

    protected virtual void OnEnable()
    {
        agent.enabled = true;
    }

    protected virtual void OnDisable()
    {
        agent.enabled = false;
    }

    protected void MoveDir(Vector3 dir)
    {
        StopMoveDirTime();
        this.dir = dir;
        if (moveDir == null)
            moveDir = StartCoroutine("MoveDirCoroutine");
    }

    protected void StopMoveDir()
    {
        if (moveDir == null) return;
        StopCoroutine("MoveDirCoroutine");
        moveDir = null;
        head.animator.SetBool("Moving", false);
    }


    Coroutine moveDir = null;
    Vector3 dir;
    IEnumerator MoveDirCoroutine()
    {
        head.animator.SetBool("Moving", true);

        while (true)
        {
            transform.rotation = Quaternion.LookRotation(dir);
            transform.Translate(Vector3.forward * head.speedMoving * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }




    public void StartMoveDirTime(Vector3 dir, float time)
    {
        StopMoveDir();
        if (moveDirTime != null) StopCoroutine(moveDirTime);
        moveDirTime = StartCoroutine(MoveDirTime(dir, time));
    }

    public void StopMoveDirTime()
    {
        if (moveDirTime == null) return;
        StopCoroutine(moveDirTime);
        moveDirTime = null;
        head.animator.SetBool("Moving", false);
    }

    Coroutine moveDirTime = null;
    IEnumerator MoveDirTime(Vector3 dir, float time)
    {
        head.animator.SetBool("Moving", true);
        float t = 0;
        transform.rotation = Quaternion.LookRotation(dir);

        while (t < time)
        {
            t += Time.deltaTime;
            transform.Translate(Vector3.forward * head.speedMoving * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        head.animator.SetBool("Moving", false);
    }
}
