using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class DelayEvent : MonoBehaviour
    {
        public UnityEvent delayEvent;
        
        public float delay;

        public void CallDelay()
        {
            StartCoroutine(DelayInvoke());
        }
        IEnumerator DelayInvoke()
        {
            yield return new WaitForSeconds(delay);
            delayEvent.Invoke();
            gameObject.SetActive(false);
        }
    }
}
