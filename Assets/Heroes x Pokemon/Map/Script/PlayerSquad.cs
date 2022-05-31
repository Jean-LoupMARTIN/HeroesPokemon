using UnityEngine;




public class PlayerSquad : Squad
{
    static public PlayerSquad inst;

    protected override void Awake()
    {
        inst = this;
        base.Awake();
    }

    private void Update()
    {
        Vector3 dir = Vector3.zero;

        if (Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.Q) ||
            Input.GetKey(KeyCode.Z) ||
            Input.GetKey(KeyCode.S))
        {
            Vector3 rightCam   = Camera.main.transform.right;
            Vector3 forwardCam = Camera.main.transform.forward;
            forwardCam.y = 0;
            forwardCam.Normalize();

            if (Input.GetKey(KeyCode.D)) dir += rightCam;
            if (Input.GetKey(KeyCode.Q)) dir -= rightCam;
            if (Input.GetKey(KeyCode.Z)) dir += forwardCam;
            if (Input.GetKey(KeyCode.S)) dir -= forwardCam;            
        }

        if (dir != Vector3.zero) MoveDir(dir);
        else StopMoveDir();
    }
}
