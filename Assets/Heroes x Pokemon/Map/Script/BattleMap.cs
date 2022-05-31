using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMap : MonoBehaviour
{
    public static BattleMap inst;

    int[,] battleMap;
    int xMap, yMap;

    BattleTile[,] tilesMap;
    public BattleTile tilePrefab;

    public int battleSize = 11; // odd number !
    public int nbLinesStartBattle = 2;

    Vector3 p1Side;

    List<Mob> p1Mobs, p2Mobs;
    public MobInfos p1MobsInfo, p2MobsInfo;
    Squad p1Squad, p2Squad;

    //Mob selectedMob = null;
    //MobInfo selectedInfo = null;

    //Dictionary<Mob, float> mobsIni;
    //List<Mob> mobsOrder;
    public int mobsOrderSize = 10;
    //public MobsOrder mobsOrderDisplay;





    void Awake()
    {
        inst = this;
    }




    #region SEARCH BATTLE SPOT

    public bool SearchStartBattle(EnemySquad enemySquad)
    {
        // player side
        Vector3 playerSide;
        float angle = Vector3.SignedAngle(Vector3.forward, Tool.Dir(PlayerSquad.inst, enemySquad), Vector3.up);
        if      (angle > -45  && angle <  45)  playerSide = Vector3.back;
        else if (angle >  45  && angle <  135) playerSide = Vector3.left;
        else if (angle > -135 && angle < -45)  playerSide = Vector3.right;
        else                                   playerSide = Vector3.forward;

        p1Mobs = PlayerSquad.inst.mobs;
        p2Mobs = enemySquad.mobs;
        p1Side = playerSide;
        p1Squad = PlayerSquad.inst;
        p2Squad = enemySquad;

        int[,] map = Map.inst.map;
        int size = map.GetLength(0);
        int x, y;

        // fly ?
        bool fly = false;
        foreach (Mob mob in PlayerSquad.inst.mobs)
            if (mob.fly) fly = true;

        bool[,] close = new bool[size, size];
        for (x = 0; x < size; x++)
            for (y = 0; y < size; y++)
                close[x, y] = false;

        Queue<(int, int)> queue = new Queue<(int, int)>();
        int xStart = (int)PlayerSquad.inst.transform.position.x;
        int yStart = (int)PlayerSquad.inst.transform.position.z;
        queue.Enqueue((xStart, yStart));
        close[xStart, yStart] = true;

        while (queue.Count > 0)
        {
            (x, y) = queue.Dequeue();
            if (TryStartBattle(x, y))
                return true;

            if (x+1 < size && !close[x+1, y] && (map[x+1, y] == 0 || (fly && map[x+1, y] == -1))) { queue.Enqueue((x+1, y)); close[x+1, y] = true; }
            if (y+1 < size && !close[x, y+1] && (map[x, y+1] == 0 || (fly && map[x, y+1] == -1))) { queue.Enqueue((x, y+1)); close[x, y+1] = true; }
            if (x-1 >= 0   && !close[x-1, y] && (map[x-1, y] == 0 || (fly && map[x-1, y] == -1))) { queue.Enqueue((x-1, y)); close[x-1, y] = true; }
            if (y-1 >= 0   && !close[x, y-1] && (map[x, y-1] == 0 || (fly && map[x, y-1] == -1))) { queue.Enqueue((x, y-1)); close[x, y-1] = true; }
        }

        return false;
    }

    bool TryStartBattle(int xCenter, int yCenter)
    {
        int[,] map = Map.inst.map;
        int size = map.GetLength(0);

        xMap = xCenter - battleSize/2;
        yMap = yCenter - battleSize/2;

        // check battleMap in map
        if (xMap < 0 || xMap + battleSize >= size ||
            yMap < 0 || yMap + battleSize >= size)
            return false;

        // init battleMap
        battleMap = new int[battleSize, battleSize];
        for (int x = 0; x < battleSize; x++)
            for (int y = 0; y < battleSize; y++)
                battleMap[x, y] = map[xMap + x, yMap + y];

        RemoveBattleMapIslands();
        if (!Check0onBorders()) return false;
        if (!CheckBigAccess()) return false;
        if (!CheckMobsStart(out Vector2[] p1MobsStart, out Vector2[] p2MobsStart)) return false;

        StartBattle(p1MobsStart, p2MobsStart);
        return true;
    }

    // -2 for island
    void RemoveBattleMapIslands()
    {
        // generate islandsMap
        int[,] islandsMap = new int[battleSize, battleSize];

        for (int x = 0; x < battleSize; x++)
            for (int y = 0; y < battleSize; y++)
                islandsMap[x, y] = -1;


        int id = 0;
        for (int x = 0; x < battleSize; x++) {
            for (int y = 0; y < battleSize; y++)
            {
                if (islandsMap[x, y] == -1 && battleMap[x, y] == 0)
                {
                    Queue<(int, int)> queue = new Queue<(int, int)>();
                    queue.Enqueue((x, y));
                    islandsMap[x, y] = id;

                    int x2, y2;
                    while (queue.Count > 0)
                    {
                        (x2, y2) = queue.Dequeue();
                        if (x2+1 < battleSize && battleMap[x2+1, y2] == 0 && islandsMap[x2+1, y2] == -1) { queue.Enqueue((x2+1, y2)); islandsMap[x2+1, y2] = id; }
                        if (y2+1 < battleSize && battleMap[x2, y2+1] == 0 && islandsMap[x2, y2+1] == -1) { queue.Enqueue((x2, y2+1)); islandsMap[x2, y2+1] = id; }
                        if (x2-1 >= 0         && battleMap[x2-1, y2] == 0 && islandsMap[x2-1, y2] == -1) { queue.Enqueue((x2-1, y2)); islandsMap[x2-1, y2] = id; }
                        if (y2-1 >= 0         && battleMap[x2, y2-1] == 0 && islandsMap[x2, y2-1] == -1) { queue.Enqueue((x2, y2-1)); islandsMap[x2, y2-1] = id; }
                    }

                    id++;
                }

            }
        }

        if (id < 2) return;

        // calculate islands size
        int[] islandsSize = new int[id];
        for (int i = 0; i < id; i++)
            islandsSize[i] = 0;

        for (int x = 0; x < battleSize; x++)
            for (int y = 0; y < battleSize; y++)
                if (islandsMap[x, y] != -1)
                    islandsSize[islandsMap[x, y]]++;


        // pick best islands
        int bestIsland = -1;
        int bestIslandSize = -1;
        for (int i = 0; i < id; i++)
        {
            if (islandsSize[i] > bestIslandSize)
            {
                bestIsland = i;
                bestIslandSize = islandsSize[i];
            }
        }

        for (int x = 0; x < battleSize; x++)
            for (int y = 0; y < battleSize; y++)
                if (islandsMap[x, y] != -1 && islandsMap[x, y] != bestIsland)
                    battleMap[x, y] = -2;
    }

    private bool Check0onBorders()
    {
        bool left0  = false;
        bool right0 = false;
        bool up0    = false;
        bool down0  = false;

        for (int i = 0; i < battleSize; i++)
        {
            if (battleMap[i, 0] == 0)             down0  = true;
            if (battleMap[0, i] == 0)             left0  = true;
            if (battleMap[i, battleSize-1] == 0)  up0    = true;
            if (battleMap[battleSize-1, i] == 0)  right0 = true;
        }

        return left0 && right0 && up0 && down0;
    }

    bool CheckBigAccess() 
    {
        int x, y;
        bool[,] close = new bool[battleSize-1, battleSize-1];
        for (x = 0; x < battleSize-1; x++)
            for (y = 0; y < battleSize-1; y++)
                close[x, y] = false;

        Queue<(int, int)> queue = new Queue<(int, int)>();
        for (x = 0; x < battleSize-1 && queue.Count == 0; x++)
            for (y = 0; y < battleSize-1 && queue.Count == 0; y++)
                if (CheckBigMob(x, y, battleMap)) { queue.Enqueue((x, y)); close[x, y] = true; }

        bool[,] caseToAccess = new bool[battleSize, battleSize];
        for (x = 0; x < battleSize; x++)
            for (y = 0; y < battleSize; y++)
                caseToAccess[x, y] = battleMap[x, y] == 0;

        while (queue.Count > 0)
        {
            (x, y) = queue.Dequeue();

            for (int x2 = x-1; x2 <= x+2; x2++)
                for (int y2 = y-1; y2 <= y+2; y2++)
                    if (x2 >= 0 && x2 < battleSize &&
                        y2 >= 0 && y2 < battleSize)
                        caseToAccess[x2, y2] = false;

            for (int x2 = x-1; x2 <= x+1; x2++)
                for (int y2 = y-1; y2 <= y+1; y2++)
                    if (CheckBigMob(x2, y2, battleMap) && !close[x2, y2])   { queue.Enqueue((x2, y2)); close[x2, y2] = true; }
        }

        for (x = 0; x < battleSize; x++)
            for (y = 0; y < battleSize; y++)
                if (caseToAccess[x, y]) return false;

        return true;
    }

    bool CheckMobsStart(out Vector2[] p1MobsStart, out Vector2[] p2MobsStart)
    {
        p1MobsStart = null;
        p2MobsStart = null;
        if (!TryMobsStart(p1Mobs, p1Side, out p1MobsStart)) return false;
        if (!TryMobsStart(p2Mobs,-p1Side, out p2MobsStart)) return false;
        return true;
    }

    bool TryMobsStart(List<Mob> mobs, Vector3 side, out Vector2[] mobsStart)
    {
        mobsStart = null;
        for (int i = 0; i < 100; i++)
            if (TryRandMobsStart(mobs, side, out mobsStart))
                return true;
        return false;
    }

    bool TryRandMobsStart(List<Mob> mobs, Vector3 side, out Vector2[] mobsStart)
    {
        mobsStart = new Vector2[mobs.Count];

        int[,] close = new int[battleSize, battleSize];
        for (int x = 0; x < battleSize; x++)
            for (int y = 0; y < battleSize; y++)
                close[x, y] = 0;


        // big
        for (int i = 0; i < mobs.Count; i++)
        {
            if (!mobs[i].big) continue;

            bool find = false;
            for (int j = 0; j < 100 && !find; j++)
            {
                int x = Tool.Rand(battleSize);
                int y = Tool.Rand(nbLinesStartBattle-1);

                if      (side == Vector3.forward) y += battleSize - nbLinesStartBattle;
                else if (side == Vector3.left)    (x, y) = (y, x);
                else if (side == Vector3.right) { (x, y) = (y, x); x += battleSize - nbLinesStartBattle; }

                if (CheckBigMob(x, y, battleMap) && CheckBigMob(x, y, close))
                {
                    mobsStart[i] = new Vector2(x, y);
                    close[x, y] = 1;
                    close[x+1, y] = 1;
                    close[x, y+1] = 1;
                    close[x+1, y+1] = 1;
                    find = true;
                }
            }

            if (!find) return false;
        }

        // small
        for (int i = 0; i < mobs.Count; i++)
        {
            if (mobs[i].big) continue;

            bool find = false;
            for (int j = 0; j < 100 && !find; j++)
            {
                int x = Tool.Rand(battleSize);
                int y = Tool.Rand(nbLinesStartBattle);

                if      (side == Vector3.forward) y += battleSize - nbLinesStartBattle;
                else if (side == Vector3.left)    (x, y) = (y, x);
                else if (side == Vector3.right) { (x, y) = (y, x); x += battleSize - nbLinesStartBattle; }

                if (battleMap[x, y] == 0 && close[x, y] == 0)
                {
                    mobsStart[i] = new Vector2(x, y);
                    close[x, y] = 1;
                    find = true;
                }
            }

            if (!find) return false;
        }

        return true;
    }


    #endregion


    #region BATTLE



    public void StartBattle(Vector2[] p1MobsStart, Vector2[] p2MobsStart)
    {
        // desactive squads
        PlayerSquad.inst.gameObject.SetActive(false);
        foreach (EnemySquad e in EnemySquad.list)
            e.gameObject.SetActive(false);

        // active p1 p2 squads than disable
        p1Squad.gameObject.SetActive(true);
        p2Squad.gameObject.SetActive(true);
        p1Squad.enabled = false;
        p2Squad.enabled = false;

        // desactive and turn p1 p2 mobs
        foreach (Mob mob in p1Mobs) { mob.gameObject.SetActive(false); mob.transform.forward = -p1Side; }
        foreach (Mob mob in p2Mobs) { mob.gameObject.SetActive(false); mob.transform.forward =  p1Side; }

        // place camera
        MapCamera.inst.StopLookPlayer();
        MapCamera.inst.LookPoint(new Vector3(xMap + (float)battleSize/2, 0, yMap + (float)battleSize/2));

        // generate tiles
        tilesMap = new BattleTile[battleSize, battleSize];
        for (int x = 0; x < battleSize; x++) { 
            for (int y = 0; y < battleSize; y++)
            {
                tilesMap[x, y] = battleMap[x, y] == 0 ? Instantiate(tilePrefab, new Vector3(xMap + x, 0, yMap + y), Quaternion.identity) : null;
                if (tilesMap[x, y]) tilesMap[x, y].SetState(1);
            }
        }

        // mobs info
        PlayerSquadInfo.inst.gameObject.SetActive(false);
        p1MobsInfo.gameObject.SetActive(true);
        p2MobsInfo.gameObject.SetActive(true);
        p1MobsInfo.SetMobs(p1Mobs, false);
        p2MobsInfo.SetMobs(p2Mobs, false);

        p1MobsInfo.SetAllState(0);
        p2MobsInfo.SetAllState(0);

        StartPlaceMobs(true, p1MobsStart, p2MobsStart);
    }


   
    
    void StartPlaceMobs(bool p1, Vector2[] p1MobsStart, Vector2[] p2MobsStart)
    {
        Vector3 side = p1 ? p1Side : -p1Side;
        List<Mob> mobs = p1 ? p1Mobs : p2Mobs;
        Vector2[] mobsStart = p1 ? p1MobsStart : p2MobsStart;

        for (int i = 0; i < mobs.Count; i++)
        {
            Mob mob = mobs[i];
            Vector2 coor = mobsStart[i];
            mob.gameObject.SetActive(true);
            mob.transform.position = new Vector3(xMap + coor.x + (mob.big ? 1 : 0.5f), 0, yMap + coor.y + (mob.big ? 1 : 0.5f));
        }

        (p1 ? p1MobsInfo : p2MobsInfo).SetAllState(1);

        if (p1) StartPlaceMobs(false, p1MobsStart, p2MobsStart);
        else StartCoroutine("KillEnemyOnSpace");



    }

    IEnumerator KillEnemyOnSpace()
    {
        while (!Input.GetKey(KeyCode.Space))
            yield return new WaitForEndOfFrame();

        for (int x = 0; x < battleSize; x++)
            for (int y = 0; y < battleSize; y++)
                if (tilesMap[x, y]) Destroy(tilesMap[x, y].gameObject);


        foreach (Mob mob in p2Mobs)
        {
            mob.transform.SetParent(null);
            while (mob.life > 0) mob.TakeDamage(10, -p1Side);
        }

        DestroyImmediate(p2Squad.gameObject);

        p1MobsInfo.SetMobs(null);
        p2MobsInfo.SetMobs(null);
        p1MobsInfo.gameObject.SetActive(false);
        p2MobsInfo.gameObject.SetActive(false);
        PlayerSquadInfo.inst.gameObject.SetActive(true);


        for(int i = 0; i < PlayerSquad.inst.mobs.Count; i++)
        {
            Mob mob = PlayerSquad.inst.mobs[i];
            mob.transform.localPosition = Vector3.zero;
            mob.transform.localRotation = Quaternion.identity;
            mob.gameObject.SetActive(i == 0);
        }

        PlayerSquad.inst.enabled = true;
        MapCamera.inst.StartLookPlayer();

        foreach (EnemySquad e in EnemySquad.list)
        {
            e.gameObject.SetActive(true);
            e.StartCoroutine("RandMove");
            e.StartCoroutine("SearchPlayer");
        }
    }


    /*
        // tile
        for (int x = 0; x < battleSize; x++) {
            for (int y = 0; y < nbLinesStartBattle; y++)
            {
                if      (side == Vector3.back)      tilesMap[x, y]             ?.SetState(1);
                else if (side == Vector3.forward)   tilesMap[x, battleSize-1-y]?.SetState(1);
                else if (side == Vector3.left)      tilesMap[y, x]             ?.SetState(1);
                else                                tilesMap[battleSize-1-y, x]?.SetState(1);
            }
        }

        // enable selectColliders
        foreach (Mob mob in p1 ? p1Mobs : p2Mobs) mob.selectCollider.enabled = true;

        // mobs info
        MobsInfo mobsInfo = p1 ? p1MobsInfo : p2MobsInfo;
        foreach (MobInfo info in mobsInfo.infos)
        {
            info.enterEvent.AddListener(() => Over(true,  info.mob, info));
            info.exitEvent .AddListener(() => Over(false, info.mob, info));
            info.downEvent .AddListener(() => Select(info.mob, info));

            info.mob.enterEvent.AddListener(() => Over(true,  info.mob, info));
            info.mob.exitEvent .AddListener(() => Over(false, info.mob, info));
            info.mob.downEvent .AddListener(() => Select(info.mob, info));
        }


        buttonOK.gameObject.SetActive(true);
        buttonOK.onClick.RemoveAllListeners();

        if (p1) buttonOK.onClick.AddListener(() => StartPlaceMobs(false));
        else    buttonOK.onClick.AddListener(() => buttonOK.gameObject.SetActive(false));

    void Over(bool over, Mob mob, MobInfo info)
    {
        info.SetState(over ? 1 : 0);
        mob.outline.enabled = over;
    }


    void Select(Mob mob, MobInfo info)
    {
        if (mob == selectedMob) return;

        if (selectedMob) Unselect();

        selectedMob = mob;
        selectedInfo = info;

        if (mob == null) return;

        info.enterEvent.RemoveAllListeners();
        info.exitEvent .RemoveAllListeners();
        info.downEvent .RemoveAllListeners();

        mob.enterEvent.RemoveAllListeners();
        mob.exitEvent.RemoveAllListeners();
        mob.downEvent.RemoveAllListeners();

        mob.outline.enabled = false;
        info.SetState(2);

        for (int x = 0; x < battleSize; x++) {
            for (int y = 0; y < battleSize; y++) {
                if (tilesMap[x, y] && tilesMap[x, y].state == 1)
                {
                    int xtmp = x;
                    int ytmp = y;
                    tilesMap[x, y].enterEvent.AddListener(() => MoveSelectedMob(xtmp, ytmp));
                }
            }
        }

        mob.selectCollider.enabled = false;

        StartCoroutine("UnselectOnMouseUp");
    }



    void Unselect()
    {
        if (!selectedMob) return;

        Mob mob = selectedMob;
        MobInfo info = selectedInfo;

        info.enterEvent.AddListener(() => Over(true,  mob, info));
        info.exitEvent .AddListener(() => Over(false, mob, info));
        info.downEvent .AddListener(() => Select(mob, info));
        
        mob.enterEvent.AddListener(() => Over(true,  mob, info));
        mob.exitEvent .AddListener(() => Over(false, mob, info));
        mob.downEvent .AddListener(() => Select(mob, info));

        info.SetState(0);

        for (int x = 0; x < battleSize; x++)
            for (int y = 0; y < battleSize; y++)
                if (tilesMap[x, y]) tilesMap[x, y].enterEvent.RemoveAllListeners();

        mob.selectCollider.enabled = true;

        selectedMob = null;
        selectedInfo = null;
    }

    void MoveSelectedMob(int x, int y)
    {
        selectedMob.gameObject.SetActive(true);
        selectedMob.transform.position = new Vector3(xMap + x + (selectedMob.big ? 1 : 0.5f), 0, yMap + y + (selectedMob.big ? 1 : 0.5f));
    }

    IEnumerator UnselectOnMouseUp()
    {
        while (Input.GetMouseButton(0))
            yield return new WaitForEndOfFrame();

        Unselect();
    }
    */











    // mobs order
    /*
    mobsIni = new Dictionary<Mob, float>();
    mobs1 = new HashSet<Mob>();
    mobs2 = new HashSet<Mob>();
    foreach (Mob m in s1.mobs) { mobsIni.Add(m, Tool.Rand(1f) / m.initiative); mobs1.Add(m); }
    foreach (Mob m in s2.mobs) { mobsIni.Add(m, Tool.Rand(1f) / m.initiative); mobs2.Add(m); }

    mobsOrderDisplay.gameObject.SetActive(true);
    UpdateMobsOrders();
  
    void UpdateMobsOrders()
    {
        mobsOrder = new List<Mob>();

        Dictionary<Mob, float> mobsIniClone = new Dictionary<Mob, float>();

        foreach (KeyValuePair<Mob, float> pair in mobsIni)
            mobsIniClone.Add(pair.Key, pair.Value);

        for (int i = 0; i < mobsOrderSize; i++)
        {
            Mob mobIniMin = null;
            float iniMin = Mathf.Infinity;

            foreach (KeyValuePair<Mob, float> pair in mobsIniClone)
                if (pair.Value < iniMin)
                    (mobIniMin, iniMin) = (pair.Key, pair.Value);

            mobsIniClone[mobIniMin] += 1f / mobIniMin.initiative;

            mobsOrder.Add(mobIniMin);
        }


        mobsOrderDisplay.Set(mobsOrder, mobs1, mobs2);
    }
    */



    #endregion




    static public bool CheckBigMob(int x, int y, int[,] map)
    {
        return x >= 0 && x+1 < map.GetLength(0) &&
               y >= 0 && y+1 < map.GetLength(1) &&
               map[x, y] == 0 &&
               map[x+1, y] == 0 &&
               map[x, y+1] == 0 &&
               map[x + 1, y+1] == 0;
    }
}

