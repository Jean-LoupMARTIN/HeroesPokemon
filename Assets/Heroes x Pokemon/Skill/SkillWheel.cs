using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillWheel : MonoBehaviour
{
    public float radiusStart = 0.5f, radiusEnd = 1;

    private void OnDrawGizmos()
    {
        UpdateArcs();
    }


    void UpdateArcs()
    {
        float arcAngle = 360 / transform.childCount;

        for (int i = 0; i < transform.childCount; i++)
        {
            SkillArc arc = transform.GetChild(i).GetComponent<SkillArc>();
            arc.angle = arcAngle;
            arc.radiusStart = radiusStart;
            arc.radiusEnd = radiusEnd;
            arc.transform.localPosition = Vector3.zero;
            arc.transform.localRotation = Quaternion.Euler(Vector3.forward * 360 * i / transform.childCount);
        }
    }
}


/*
nbLinesStartBattle++
battleSize += 2

bonus de def s'applique egalement sur l att, ...

Light grace de force / ini / def

Fire :
Marche de feu

Knowledge :
donne un partie de son xp au mob le plus faible de l'equipe
oublie toutes les skills liée a knowledge pour gros bonus 
sort qui fait oublier un / tous arc de l'adversair pd nb tour
fait oublier un arc au serviteur le plus faible qui redecouvre autant de talent ? pb avec lien, utile pour escanor evo spe pas sur idee bonne

Escanor :
Rage : +20% dmg +20% ini +40% dmg recus
Rage Ultime : commence le combat en etat de rage, la rage ne rend plus +fragile
Furie : +1 deplacement en rage

Attack / Fire / Light :
Enfant du Soleil -> Escanor Evolution spe

Than
att dist foudre

item :
+ att def, ini, deplacement
niv ultime (le 4eme)


 */