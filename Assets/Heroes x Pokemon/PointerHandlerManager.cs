using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerHandlerManager : MonoBehaviour
{
    PointerHandler overMem;
    
    private void Update()
    {
        PointerHandler over = null;

        if (Tool.MouseRaycast(out RaycastHit hit))
            over = Tool.SearchComponent<PointerHandler>(hit.transform);

        if (over != overMem)
        {
            overMem?.exitEvent.Invoke();
            over?.enterEvent.Invoke();
            overMem = over;
        }

        if (over)
        {
            over.overEvent.Invoke();
            if (Input.GetMouseButtonDown(0)) over.downEvent.Invoke();
            if (Input.GetMouseButtonUp  (0)) over.upEvent  .Invoke();
        }


    }
}
