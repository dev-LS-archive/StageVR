using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Dev_LSG.Scripts.Event
{
    public class CollEventTagCount : MonoBehaviour
    {
        public string tagStr;
        public UnityEvent[] collEvents;
        public bool eventDone = false;

        [SerializeField] private int count = 0;

        // private void OnCollisionEnter(Collision collision)
        // {
        //     if (collision.gameObject.CompareTag(tagStr))
        //     {
        //         if (eventDone == false)
        //         {
        //             //print(collision.gameObject.tag);
        //             //eventDone = true;
        //             collEvents[count].Invoke();
        //             count++;
        //             if (count >= collEvents.Length)
        //             {
        //                 eventDone = true;
        //             }
        //         }
        //     }
        // }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(tagStr))
            {
                if (eventDone == false)
                {
                    //print(other.gameObject.tag);
                    //eventDone = true;
                    collEvents[count].Invoke();
                    count++;
                    if (count >= collEvents.Length)
                    {
                        eventDone = true;
                    }
                }
            }
        }
    }
}
