using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class CollEventTagSimple : MonoBehaviour
    {
        public string tagStr;
        public UnityEvent collEvent;
        private float _fillAmount;
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(tagStr))
            {
                print("Coll " + collision.gameObject.name);
                collEvent.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(tagStr))
            {
                print("Trigger " + other.gameObject.name);
                collEvent.Invoke();
            }
        }
    }
}
