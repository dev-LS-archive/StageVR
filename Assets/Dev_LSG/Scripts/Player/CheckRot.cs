using System;
using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Player
{
    public class CheckRot : MonoBehaviour
    {
        public UnityEvent plusEvent;
        public UnityEvent minusEvent;

        // private void Update()
        // {
        //     print(transform.rotation.y);
        // }

        [ContextMenu("Rot")]
        public void RotEvent()
        {
            var rot = transform.rotation.y;
            if (rot >= 0)
            {
                print("+");
                plusEvent.Invoke();
            }
            else
            {
                print("-");
                minusEvent.Invoke();
            }
        }
    }
}
