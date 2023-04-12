using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class FishEvent : MonoBehaviour
    {
        public UnityEvent fishEvent;
        [SerializeField] private bool hooked = false;

        private void OnTriggerEnter(Collider other)
        {
            if (hooked == false)
            {
                if (other.CompareTag("Hook"))
                {
                    hooked = true;
                    fishEvent.Invoke();
                }
            }
        }

        public void ResetHooked()
        {
            hooked = false;
        }
    }
}
