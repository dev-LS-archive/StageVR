using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class AudioEvent : MonoBehaviour
    {
        public UnityEvent disableEvent;
        public AudioSource audioSource;

        public bool delayEvent = false;
        public float delay;
        private bool _coolTime = false;
        void Update()
        {
            if (delayEvent == false)
            {
                if (!audioSource.isPlaying)
                {
                    disableEvent.Invoke();
                    gameObject.SetActive(false);
                }
            }
            else
            {
                if (_coolTime == false)
                {
                    if (!audioSource.isPlaying)
                    {
                        _coolTime = true;
                        CallDelay();
                    }
                }
            }
        }

        public void CallDelay()
        {
            StartCoroutine(DelayInvoke());
        }
        IEnumerator DelayInvoke()
        {
            yield return new WaitForSeconds(delay);
            disableEvent.Invoke();
        }
    }
}
