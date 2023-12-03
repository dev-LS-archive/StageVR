using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Interactables
{
    public class ResetTr : MonoBehaviour
    {
        public Transform resetTransform;
        public UnityEvent resetEvent;
        public bool canReset = true;

        [ContextMenu("ResetTR")]
        public void Reset()
        {
            if (canReset)
            {
                var tr = transform;
                tr.position = resetTransform.position;
                tr.rotation = resetTransform.rotation;
                resetEvent.Invoke();
            }
        }

        public void SetReset(bool can)
        {
            canReset = can;
        }
    }
}
