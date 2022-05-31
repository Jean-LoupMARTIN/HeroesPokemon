using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    static public MapCamera inst;

    public float dist = 8;
    public float rotSpeed = 100;
    public float xRotMin = 30, xRotMax = 60;

    [HideInInspector]
    public Vector3 lookPoint;

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        StartLookPlayer();
    }

    private void Update()
    {
        int turnRight = 0;
        if (Input.GetKey(KeyCode.LeftArrow))  turnRight--;
        if (Input.GetKey(KeyCode.RightArrow)) turnRight++;
        if (turnRight != 0)
        {
            Vector3 eulerAngles = transform.eulerAngles + Vector3.up * turnRight * Time.deltaTime * rotSpeed;
            transform.eulerAngles = eulerAngles;
            LookPoint(lookPoint);
        }

        int turnUp = 0;
        if (Input.GetKey(KeyCode.DownArrow)) turnUp--;
        if (Input.GetKey(KeyCode.UpArrow))   turnUp++;
        if (turnUp != 0)
        {
            Vector3 eulerAngles = transform.eulerAngles + Vector3.right * turnUp * Time.deltaTime * rotSpeed;
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, xRotMin, xRotMax);
            transform.eulerAngles = eulerAngles;
            LookPoint(lookPoint);

        }
    }

    public void StartLookPlayer() => StartCoroutine("LookPlayer");
    public void StopLookPlayer()  => StopCoroutine("LookPlayer");


    IEnumerator LookPlayer()
    {
        while (true)
        {
            LookPoint(PlayerSquad.inst.transform.position);
            yield return new WaitForEndOfFrame();
        }
    }

    public void LookPoint(Vector3 point)
    {
        lookPoint = point;
        transform.position = point;
        transform.Translate(Vector3.back * dist);
    }
}
