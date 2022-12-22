using UnityEngine;

namespace Dev_LSG.Scripts.Event
{
    public class RigCollection : MonoBehaviour
    {
        public Rigidbody[] rigs;

        public void SetKinematic(bool value)
        {
            foreach (var rig in rigs)
            {
                rig.isKinematic = value;
            }
        }
    }
}
