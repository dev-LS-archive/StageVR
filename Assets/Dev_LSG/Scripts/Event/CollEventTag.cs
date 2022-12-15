using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class CollEventTag : MonoBehaviour
    {
        public string tagStr;
        public UnityEvent collEvent;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(tagStr))
            {
                collEvent.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(tagStr))
            {
                collEvent.Invoke();
            }
        }
    }
}
