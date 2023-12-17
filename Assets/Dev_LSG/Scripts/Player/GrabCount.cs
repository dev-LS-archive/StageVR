using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Player
{
    public class GrabCount : MonoBehaviour
    {
        private int _count = 0;
        private bool _eventDone;

        public UnityEvent twoGrabEvent;
        public UnityEvent releasedAllEvent;

        private void OnEnable()
        {
            _count = 0;
            _eventDone = false;
        }

        public void Count()
        {
            _count += 1;
            if (_count == 2 && !_eventDone)
            {
                _eventDone = true;
                twoGrabEvent.Invoke();
            }
        }

        public void DisCount()
        {
            _count -= 1;
            if (_count == 0)
            {
                releasedAllEvent.Invoke();
            }
        }

        public void ResetEvent()
        {
            _eventDone = false;
        }
    }
}
