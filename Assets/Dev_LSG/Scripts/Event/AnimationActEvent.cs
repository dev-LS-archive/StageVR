using UnityEngine;
using UnityEngine.Events;


namespace Dev_LSG.Scripts.Event
{
    public class AnimationActEvent : MonoBehaviour
    {
        public GameObject obj;
        public UnityEvent animEvent;

        public void AnimEvent()
        {
            animEvent.Invoke();
        }
        public void Act()
        {
            //print("act");
            obj.SetActive(true);
        }
    }
}
