using System;
using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class AudioEvent : MonoBehaviour
    {
        public UnityEvent disableEvent;
        public AudioSource audioSource;
        void Update()
        {
            if (!audioSource.isPlaying)
            {
                disableEvent.Invoke();
                gameObject.SetActive(false);
            }
        }
    }
}
