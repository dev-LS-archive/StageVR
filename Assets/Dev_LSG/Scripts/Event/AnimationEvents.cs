using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class AnimationEvents : MonoBehaviour
    {
        public UnityEvent[] animEvent;

        public void AnimEvent(int num)
        {
            animEvent[num].Invoke();
        }
    }
}
