using UnityEngine;

namespace Dev_LSG.Scripts.Event
{
    public class SetGravity : MonoBehaviour
    {
        public void GravityScale(float scale)
        {
            Physics.gravity = new Vector3(0, scale, 0);
        }
    }
}
