using UnityEngine;
using UnityEngine.Events;

public class PointerHandler : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent enterEvent, exitEvent, downEvent, upEvent, overEvent;
}
