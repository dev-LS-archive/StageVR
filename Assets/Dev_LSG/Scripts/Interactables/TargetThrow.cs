using System;
using UnityEngine;
using DG.Tweening;

namespace Dev_LSG.Scripts.Interactables
{
    public class TargetThrow : MonoBehaviour
    {
        public Transform target;
        // Start is called before the first frame update
        private void OnEnable()
        {
            transform.DOMove(target.position, 3);
        }
    }
}
