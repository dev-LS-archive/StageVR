using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class CollEventLayer : MonoBehaviour
    {
        public string layer;
        public UnityEvent collEvent;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer(layer))
            {
                collEvent.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(layer))
            {
                collEvent.Invoke();
            }
        }
    }
}
