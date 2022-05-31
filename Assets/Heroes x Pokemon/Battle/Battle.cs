
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Battle : MonoBehaviour
{
    static public Battle inst;

    public BattleTile tilePrefab;
    public MobInfos p1Infos, p2Infos;

    static public int[,] battleMap;
    static public BattleTile[,] tilesMap;
    static public Mob[,] mobsMap;
    static public int w, h;
    static int x, y, x2, y2, i; // cache
    static public Player p1, p2;
    public Button validateButton;

    static Dictionary<Mob, float> mobsIni;
    static List<Mob> mobsOrder;

    // tmp
    public Transform p1Trans, p2Trans; 
    public Vector2[] p1MobsStart, p2MobsStart;


    private void Awake()
    {
        inst = this;
        validateButton.gameObject.SetActive(false);
    }


    private void Start()
    {
        StartBattle(
            GetComponent<MapGenerator>().map,
            p1Trans.GetComponentsInChildren<Mob>().ToList(),
            p2Trans.GetComponentsInChildren<Mob>().ToList(),
            Vector3.back,
            p1MobsStart,
            p2MobsStart);
    }


    public void StartBattle(int [,] battleMap, List<Mob> p1Mobs, List<Mob> p2Mobs, Vector3 p1Side, Vector2[] p1MobsStart, Vector2[] p2MobsStart)
    {
        Battle.battleMap = battleMap;
        w = battleMap.GetLength(0);
        h = battleMap.GetLength(1);

        // tileMap
        tilesMap = new BattleTile[w, h];
        for (x = 0; x < w; x++) 
            for (y = 0; y < h; y++) 
                tilesMap[x, y] = battleMap[x, y] == 0 ? Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform) : null;

        mobsMap = new Mob[w, h];

        p1 = new Player(p1Infos, p1Mobs, p1Side, p1MobsStart, false);
        p2 = new Player(p2Infos, p2Mobs,-p1Side, p2MobsStart, false);

        // cam side
        Vector3 camSide;
        if      (p1Side == Vector3.back)    camSide = Vector3.right;
        else if (p1Side == Vector3.forward) camSide = Vector3.left;
        else if (p1Side == Vector3.left)    camSide = Vector3.back;
        else                                camSide = Vector3.forward;
        Cam.inst.firstSetPos = true;
        p1.StartPlaceMobs(() => p2.StartPlaceMobs(() => PlaceMobsEnd(camSide)));
    }





    static void PlaceMobsEnd(Vector3 camSide)
    {
        // camera
        float yRot = 0;
        if      (camSide == Vector3.forward)    yRot = 180;
        else if (camSide == Vector3.left)       yRot = 90;
        else if (camSide == Vector3.right)      yRot = -90;
        Cam.inst.SetPos(battleMap, yRot);

        // mobs order
        mobsIni = new Dictionary<Mob, float>();
        foreach (Mob m in p1.mobs) mobsIni.Add(m, Tool.Rand(1f) / m.initiative);
        foreach (Mob m in p2.mobs) mobsIni.Add(m, Tool.Rand(1f) / m.initiative);
        UpdateMobsOrders();

        MobsOrder.inst.gameObject.SetActive(true);

        // mobs move area on focus
        foreach (Mob m in Tool.Concat(p1.mobs, p2.mobs))
        {
            m.focusEvent  .AddListener(() => MobMoveArea(m, (BattleTile tile) => tile.SetState(2)));
            m.unfocusEvent.AddListener(() => MobMoveArea(m, (BattleTile tile) => tile.SetState(1)));
        }
    }


    static void UpdateMobsOrders()
    {
        mobsOrder = new List<Mob>();

        Dictionary<Mob, float> mobsIniClone = new Dictionary<Mob, float>();

        foreach (KeyValuePair<Mob, float> pair in mobsIni)
            mobsIniClone.Add(pair.Key, pair.Value);

        for (int i = 0; i < MobsOrder.inst.iconsCount; i++)
        {
            Mob mobIniMin = null;
            float iniMin = Mathf.Infinity;

            foreach (KeyValuePair<Mob, float> pair in mobsIniClone)
                if (pair.Value < iniMin)
                    (mobIniMin, iniMin) = (pair.Key, pair.Value);

            mobsIniClone[mobIniMin] += 1f / mobIniMin.initiative;

            mobsOrder.Add(mobIniMin);
        }

        MobsOrder.inst.Set(mobsOrder, p1.mobs, inst.p1Infos.backgroundColor, inst.p2Infos.backgroundColor);
    }



    static void MobMoveArea(Mob mob, Action<BattleTile> action)
    {
        float dx, dy;
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
                dx = mob.x - x;
                dy = mob.y - y;

                if (dx*dx + dy*dy <= mob.speed * mob.speed && tilesMap[x, y])
                    action.Invoke(tilesMap[x, y]);
        }
    }

    
    


    static void PlaceMob(Mob mob, int x, int y)
    {
        for (x2 = 0; x2 < w; x2++)
            for (y2 = 0; y2 < h; y2++)
                if (mobsMap[x2, y2] == mob) mobsMap[x2, y2] = null;

        if (mob.big) {
            mob.transform.position = new Vector3(x+1, 0, y+1);
            mobsMap[x, y]     = mob;
            mobsMap[x+1, y]   = mob;
            mobsMap[x, y+1]   = mob;
            mobsMap[x+1, y+1] = mob;
        }

        else {
            mob.transform.position = new Vector3(x+0.5f, 0, y+0.5f);
            mobsMap[x, y] = mob;
        }

        mob.x = x;
        mob.y = y;
    }

    static bool CanPlaceMob(Mob mob, int x, int y, int tileState = -1)
    {
        if (mob.big) return
            x >= 0 && x+1 < w &&
            y >= 0 && y+1 < h &&

            battleMap[x, y]     == 0 &&
            battleMap[x+1, y]   == 0 &&
            battleMap[x, y+1]   == 0 &&
            battleMap[x+1, y+1] == 0 &&
                   
            (!mobsMap[x, y]     || mobsMap[x, y]     == mob) &&
            (!mobsMap[x+1, y]   || mobsMap[x+1, y]   == mob) &&
            (!mobsMap[x, y+1]   || mobsMap[x, y+1]   == mob) &&
            (!mobsMap[x+1, y+1] || mobsMap[x+1, y+1] == mob) &&
            (tileState == -1 || (tilesMap[x, y]    .state == tileState &&
                                 tilesMap[x+1, y]  .state == tileState &&
                                 tilesMap[x, y+1]  .state == tileState &&
                                 tilesMap[x+1, y+1].state == tileState));
        
        else return
            x >= 0 && x < w &&
            y >= 0 && y < h &&
            battleMap[x, y] == 0 &&
            (!mobsMap[x, y] || mobsMap[x, y] == mob) &&
            (tileState == -1 || tilesMap[x, y].state == tileState);
    }




    static (int x, int y) ConvCoor(float x, float y, int size) => ((int)(x-(float)(size-1)/2), (int)(y-(float)(size-1)/2));





    public class Player
    {
        static MobInfo infoSelected = null;

        MobInfos infos;
        public List<Mob> mobs;
        Vector3 side;
        int nbLineStart = 3;
        bool isBot;

        public Player(MobInfos infos, List<Mob> mobs, Vector3 side, Vector2[] mobsStart, bool isBot)
        {
            this.infos = infos;
            this.mobs  = mobs;
            this.side  = side;
            this.isBot = isBot;

            // set infos
            infos.SetMobs(mobs, false);

            // mobs look
            foreach (Mob mob in mobs) mob.transform.forward = -side;

            // mobs start position
            for (i = 0; i < mobs.Count; i++)
            {
                int x = (int)mobsStart[i].x;
                int y = (int)mobsStart[i].y;
                mobs[i].gameObject.SetActive(true);
                PlaceMob(mobs[i], x, y);
            }

            infos.SetAllState(1);

            FocusMobs();
        }


        void FocusMobs()
        {
            foreach (MobInfo info in infos.array)
            {
                Mob mob = info.mob;

                mob.outline.enabled = true;
                mob.selectCollider.enabled = true;

                mob.enterEvent.AddListener(() => mob.Focus(true));
                mob.exitEvent .AddListener(() => mob.Focus(false));

                info.enterEvent.AddListener(() => mob.Focus(true));
                info.exitEvent .AddListener(() => mob.Focus(false));

                mob.Focus(false);
            }
        }






        public void StartPlaceMobs(Action OnValidate)
        {
            if (isBot)
            {
                OnValidate?.Invoke();
                return;
            }

            // camera
            float yRot = 0;
            if      (side == Vector3.forward)   yRot = 180;
            else if (side == Vector3.left)      yRot = 90;
            else if (side == Vector3.right)     yRot = -90;
            Cam.inst.SetPos(battleMap, yRot);

            

            // tile
            for (x = 0; x < w; x++) 
                for (y = 0; y < h; y++) 
                    tilesMap[x, y]?.SetState(
                        (side == Vector3.back    && y < nbLineStart) ||
                        (side == Vector3.forward && y >= h - nbLineStart) ||
                        (side == Vector3.left    && x < nbLineStart) ||
                        (side == Vector3.right   && x >= w - nbLineStart)
                        ? 2 : 1);

            ActiveSelection(true);

            // validate
            inst.validateButton.gameObject.SetActive(true);
            inst.validateButton.onClick.AddListener(() => {
                StopPlaceMobs();
                OnValidate?.Invoke();
            });
        }

        void StopPlaceMobs()
        {
            inst.StopAllCoroutines();
            Unselect();

            // tile
            for (x = 0; x < w; x++)
                for (y = 0; y < h; y++)
                    tilesMap[x, y]?.SetState(1);

            // select
            ActiveSelection(false);

            // validate
            inst.validateButton.onClick.RemoveAllListeners();
            inst.validateButton.gameObject.SetActive(false);
        }


        void ActiveSelection(bool b)
        {
            foreach (MobInfo info in infos.array)
            {
                Mob mob = info.mob;

                if (b) {
                    mob .downEvent.AddListener(() => Select(info));
                    info.downEvent.AddListener(() => Select(info));
                } else {
                    mob .downEvent.RemoveAllListeners();
                    info.downEvent.RemoveAllListeners();
                }
            }
        }

        void Select(MobInfo info)
        {
            if (info == infoSelected) return;
            if (infoSelected) Unselect();

            ActiveSelection(false);
            foreach (Mob mob in mobs) mob.selectCollider.enabled = false;

            info.SetState(2);

            infoSelected = info;

            inst.StartCoroutine(PlaceSelected());
        }

        void Unselect()
        {
            if (!infoSelected) return;

            ActiveSelection(true);
            foreach (Mob mob in mobs) mob.selectCollider.enabled = true;

            infoSelected.SetState(infoSelected.mob.gameObject.activeSelf ? 1 : 0);

            infoSelected = null;
        }

        IEnumerator PlaceSelected()
        {
            while (Input.GetMouseButton(0))
            {
                Mob mob = infoSelected.mob;
                bool placed = false;

                if (Tool.MouseRaycast(out RaycastHit hit, LayerMask.GetMask("BattleTile")))
                {
                    (x, y) = ConvCoor(hit.point.x, hit.point.z, mob.big ? 2 : 1);

                    if (CanPlaceMob(mob, x, y, 2))
                    {
                        placed = true;
                        PlaceMob(mob, x, y);
                        mob.gameObject.SetActive(true);
                    }
                }

                if (!placed)
                {
                    mob.gameObject.SetActive(false);
                    for (x = 0; x < w; x++)
                        for (y = 0; y < h; y++)
                            if (mobsMap[x, y] == mob) mobsMap[x, y] = null;
                }

                // block validateButton ?
                bool atLeast1Mob = false;
                foreach (Mob m in mobs)
                    if (m.gameObject.activeSelf == true)
                        atLeast1Mob = true;
                inst.validateButton.gameObject.SetActive(atLeast1Mob);


                yield return new WaitForEndOfFrame();
            }

            Unselect();
        }
    }
}





