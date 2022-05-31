using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public float speed = 1;

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))    transform.position = transform.position + Vector3.left      * speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow))   transform.position = transform.position + Vector3.right     * speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow))      transform.position = transform.position + Vector3.forward   * speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow))    transform.position = transform.position + Vector3.back      * speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Space))        transform.position = transform.position + Vector3.up        * speed * Time.deltaTime / 2;
        if (Input.GetKey(KeyCode.LeftShift))    transform.position = transform.position + Vector3.down      * speed * Time.deltaTime / 2;
    }
}
