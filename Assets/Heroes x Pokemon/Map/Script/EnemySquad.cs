using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class EnemySquad : Squad
{
    static public List<EnemySquad> list = new List<EnemySquad>();

    public float tMin = 1, tMax = 10, tMove = 1.5f;
    public float view = 5;


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Tool.GizmoDrawCircle(transform.position + Vector3.up * 0.01f, Vector3.up, view);
    }

    protected override void Awake()
    {
        base.Awake();
        list.Add(this);
    }

    private void OnDestroy()
    {
        list.Remove(this);
    }

    protected override void Start() // change to OnEnable
    {
        base.Start();
        StartCoroutine("RandMove");
        StartCoroutine("SearchPlayer");
    }

    IEnumerator SearchPlayer()
    {
        while (Tool.Dist(this, PlayerSquad.inst) > view)
            yield return new WaitForEndOfFrame();

        StartCoroutine("ChasePlayer");
    }

    IEnumerator ChasePlayer()
    {
        StopCoroutine("RandMove");

        while (Tool.Dist(this, PlayerSquad.inst) < view * 1.1f)
        {
            MoveDir(Tool.Dir(this, PlayerSquad.inst));

            if (Tool.Dist(this, PlayerSquad.inst) < Tool.GetAgentRadius(agent) + Tool.GetAgentRadius(PlayerSquad.inst.agent) + 0.2f)
                BattleMap.inst.SearchStartBattle(this);

            yield return new WaitForEndOfFrame();
        }

        StopMoveDir();
        StartCoroutine("RandMove");
        StartCoroutine("SearchPlayer");
    }


    IEnumerator RandMove()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(tMin, tMax));
            StartMoveDirTime(Quaternion.Euler(Vector3.up * Tool.Rand(360)) * Vector3.forward, tMove);
        }
    }

}
