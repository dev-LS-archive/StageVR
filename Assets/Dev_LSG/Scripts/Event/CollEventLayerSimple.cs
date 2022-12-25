using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class CollEventLayerSimple : MonoBehaviour
    {
        public string tagStr;
        public UnityEvent collEvent;
        public bool eventDone = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer(tagStr))
            {
                print(tagStr);
                if (eventDone == false)
                {
                    eventDone = true;
                    collEvent.Invoke();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(tagStr))
            {
                print(tagStr);
                print(other.gameObject.name);
                if (eventDone == false)
                {
                    eventDone = true;
                    collEvent.Invoke();
                }
            }
        }
    }
}
