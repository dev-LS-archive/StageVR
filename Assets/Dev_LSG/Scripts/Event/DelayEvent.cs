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

        public void CallInvoke()
        {
            Invoke(nameof(InvokeEvent), delay);
        }
        public void InvokeEvent()
        {
            //print( gameObject.name+" / invoke");
            delayEvent.Invoke();
        }
        IEnumerator DelayInvoke()
        {
            yield return new WaitForSeconds(delay);
            delayEvent.Invoke();
            gameObject.SetActive(false);
        }
    }
}
