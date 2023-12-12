using UnityEngine;
using RopeToolkit;

// ReSharper disable once IdentifierTypo
namespace Dev_LSG.Scripts.Interactables
{
    public class RopeAttach : MonoBehaviour
    {
        public Rope hookRope;
        public Transform attachTarget;
        private RopeConnection[] _connections;

        private void Start()
        {
            _connections = hookRope.GetComponents<RopeConnection>();
        }

        public void Attach()
        {
            _connections[2].transformSettings.transform = attachTarget.transform;
            print(("Attach"));
        }
    }
}
