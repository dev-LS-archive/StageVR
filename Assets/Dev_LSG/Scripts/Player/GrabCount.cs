using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Player
{
    public class GrabCount : MonoBehaviour
    {
        private int _count = 0;
        private bool _eventDone;

        public UnityEvent twoGrabEvent;
        public UnityEvent countZeroEvent;
        public UnityEvent countOneEvent;
        public UnityEvent countTwoEvent;

        private void OnEnable()
        {
            _count = 0;
            _eventDone = false;
        }

        public void Count()
        {
            _count += 1;
            if (_count == 1)
            {
                countOneEvent.Invoke();
            }
            if (_count == 2)
            {
                countTwoEvent.Invoke();
                if (!_eventDone)
                {
                    _eventDone = true;
                    twoGrabEvent.Invoke();
                }
            }
        }

        public void DisCount()
        {
            _count -= 1;
            if (_count == 0)
            {
                countZeroEvent.Invoke();
            }
        }
    }
}
