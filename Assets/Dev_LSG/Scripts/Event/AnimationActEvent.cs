using UnityEngine;

namespace Dev_LSG.Scripts.Event
{
    public class AnimationActEvent : MonoBehaviour
    {
        public GameObject obj;

        public void Act()
        {
            //print("act");
            obj.SetActive(true);
        }
    }
}
