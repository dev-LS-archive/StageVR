using UnityEngine;

namespace Dev_LSG.Scripts.Player
{
    public class RigPos : MonoBehaviour
    {
        public Transform rig;
        // Update is called once per frame
        void Update()
        {
            var position = transform.position;
            rig.position = new Vector3(position.x, 0, position.z);
        }
    }
}
