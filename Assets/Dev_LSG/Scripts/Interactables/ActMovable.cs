using System;
using UnityEngine;

namespace Dev_LSG.Scripts.Interactables
{
    public class ActMovable : MonoBehaviour
    {
        private Rigidbody Rigidbody { get; set; }
        private Transform _tr;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            _tr = transform;
        }
        
        void Update()
        {
            Rigidbody.position = _tr.position;
            Rigidbody.rotation = _tr.rotation;
        }
    }
}
