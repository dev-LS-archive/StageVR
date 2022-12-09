using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class FootholdResetPos : MonoBehaviour
    {
        public Transform resetPoint;
        public UnityEvent eventReset;

        public void ChangeResetPos()
        {
            var pos = transform.position;
            resetPoint.position = new Vector3(pos.x, resetPoint.position.y, pos.z);
            eventReset.Invoke();
        }
        public void ChangeResetPosAll()
        {
            var pos = transform.position;
            resetPoint.position = new Vector3(pos.x, pos.y, pos.z);
            eventReset.Invoke();
        }
    }
}
