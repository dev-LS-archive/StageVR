using UnityEngine;

namespace Dev_LSG.Scripts.Interactables
{
    public class SetParent : MonoBehaviour
    {
        public Transform parent;

        public void SetParentFunction()
        {
            transform.SetParent(parent);
        }

        public void SetParentStays(bool state)
        {
            transform.SetParent(parent, state);
        }
    }
}
