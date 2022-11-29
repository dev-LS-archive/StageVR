using UnityEngine;

namespace Dev_LSG.Scripts.Event
{
    public class FootholdResetPos : MonoBehaviour
    {
        public Transform resetPoint;

        public void ChangeResetPos()
        {
            var pos = transform.position;
            resetPoint.position = new Vector3(pos.x, resetPoint.position.y, pos.z);
        }
    }
}
