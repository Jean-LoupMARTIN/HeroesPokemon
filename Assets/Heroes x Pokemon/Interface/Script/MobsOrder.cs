using System.Collections.Generic;
using UnityEngine;

public class MobsOrder : MonoBehaviour
{
    static public MobsOrder inst;

    public int iconsCount = 8;
    public float iconSize = 100;
    public float firstIconSize = 150;
    public float padding = 5;

    public Transform backgroundFirst, background;

    MobsOrderIcon[] icons;
    public MobsOrderIcon iconPrefab;

    void Awake()
    {
        inst = this;

        // background
        backgroundFirst.localScale = Vector3.one * (firstIconSize + 2 * padding) / 100;
        backgroundFirst.transform.localPosition = Vector3.zero;

        float w = iconSize * (iconsCount - 1) + padding * iconsCount;
        background.localScale = new Vector3(w, iconSize + 2 * padding, 1) / 100;
        background.transform.localPosition = Vector3.right * (firstIconSize/2 + w/2);

        // icons
        icons = new MobsOrderIcon[iconsCount];
        for (int i = 0; i < iconsCount; i++)
        {
            icons[i] = Instantiate(iconPrefab, Vector3.zero, Quaternion.identity, transform);

            if (i == 0) {
                icons[i].transform.localPosition = Vector3.zero;
                icons[i].transform.localScale = Vector3.one * firstIconSize / 100;
            } else {
                icons[i].transform.localPosition = Vector3.right * (firstIconSize / 2 + padding / 2 + (iconSize + padding) * (i - 0.5f));
                icons[i].transform.localScale = Vector3.one * iconSize / 100;
            }
        }

        gameObject.SetActive(false);
    }


    public void Set(List<Mob> mobsOrder, List<Mob> p1Mobs, Color p1Color, Color p2Color)
    {
        for (int i = 0; i < mobsOrder.Count; i++)
            icons[i].SetMob(mobsOrder[i], p1Mobs.Contains(mobsOrder[i]) ? p1Color : p2Color);
    }
}
