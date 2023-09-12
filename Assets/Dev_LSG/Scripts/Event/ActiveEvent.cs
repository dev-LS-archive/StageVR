using System;
using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class ActiveEvent : MonoBehaviour
    {
        public UnityEvent enableEvent;

        private void OnEnable()
        {
            enableEvent.Invoke();
        }
    }
}
