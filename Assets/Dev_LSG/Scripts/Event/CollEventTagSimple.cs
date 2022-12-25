using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class CollEventTagSimple : MonoBehaviour
    {
        public string tagStr;
        public UnityEvent collEvent;
        public bool eventDone = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(tagStr))
            {
                if (eventDone == false)
                {
                    //print(collision.gameObject.tag);
                    eventDone = true;
                    collEvent.Invoke();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(tagStr))
            {
                if (eventDone == false)
                {
                    //print(other.gameObject.tag);
                    eventDone = true;
                    collEvent.Invoke();
                }
            }
        }
    }
}
