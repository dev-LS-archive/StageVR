using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Interactables
{
    public class TargetThrow : MonoBehaviour
    {
        public Transform target;

        public float duration = 3f;

        public UnityEvent startEvent;
        public UnityEvent endEvent;

        [ContextMenu("Throw")]
        public void Throw()
        {
            startEvent.Invoke();
            transform.DOMove(target.position, duration).SetEase(Ease.OutBounce);
            transform.DORotateQuaternion(target.rotation, duration);
            Invoke(nameof(EndEvent), duration);
        }

        private void EndEvent()
        {
            endEvent.Invoke();
        }
    }
}
