using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Interactables
{
    public class ResetTr : MonoBehaviour
    {
        public Transform resetTransform;
        public UnityEvent resetEvent;

        [ContextMenu("ResetTR")]
        public void Reset()
        {
            var tr = transform;
            tr.position = resetTransform.position;
            tr.rotation = resetTransform.rotation;
            resetEvent.Invoke();
        }
    }
}
