using UnityEngine;
using UnityEngine.UI;

namespace Dev_LSG.Scripts.Event
{
    public class DirectionInvoke : MonoBehaviour
    {
        public Button forward;
        public Button backward;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                forward.onClick.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                backward.onClick.Invoke();
            }
        }
    }
}
