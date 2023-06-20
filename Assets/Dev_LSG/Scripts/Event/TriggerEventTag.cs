using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class TriggerEventTag : MonoBehaviour
    {
        public string tagName;
        public UnityEvent triggerEvent;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(tagName))
            {
                triggerEvent.Invoke();
            }
        }
    }
}
