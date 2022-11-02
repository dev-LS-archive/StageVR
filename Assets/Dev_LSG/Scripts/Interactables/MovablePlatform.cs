using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Interactables
{
    public class MovablePlatform : MonoBehaviour
    {
        public bool canEvent = true;
        private Rigidbody Rigidbody { get; set; }
        public Transform[] end;
        public float speed = 3f;
        //public float delay = 5f;
        public float timeToMaxSpeed = 1f;

        public int order;

        private Vector3 _target;
        private float _speed;
        private float _timer;
        private bool _waiting = true;
        private bool _endTarget;
        private float _elapsed;
        
        public UnityEvent[] endEvent;

        public bool brake;
        public Transform audioPos;
        public AudioSource brakeSound;
    
        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            _target = end[order].position;
            _endTarget = true;
            _waiting = true;
        }

        [ContextMenu("Call Move")]
        public void CallMove()
        {
            _waiting = false;
            StartCoroutine(Moving());
        }

        IEnumerator Moving()
        {
            while (!_waiting)
            {
                _speed = Mathf.Lerp(0, speed, _elapsed / timeToMaxSpeed);

                Rigidbody.MoveRotation(Quaternion.RotateTowards(Rigidbody.rotation, end[order].rotation, _speed));
                Rigidbody.MovePosition(Vector3.MoveTowards(Rigidbody.position, _target, _speed * Time.deltaTime));

                if (brake)
                {
                    if ((audioPos.position - Rigidbody.position).magnitude < 5)
                    {
                        //print("Audio");
                        brakeSound.Play();
                    }
                }
                
                if ((_target - Rigidbody.position).magnitude < .01)
                {
                    _speed = 0f;
                    if (canEvent)
                    {
                        if (order < endEvent.Length)
                            endEvent[order].Invoke();
                    }
                        
                    print("Event");
                    order++;
                    if (order.Equals(end.Length))
                    {
                        break;
                    }
                    _target = end[order].position;
                    _waiting = true;
                    _endTarget = !_endTarget;
                }

                _timer += Time.deltaTime;
                _elapsed += Time.deltaTime;
                if (_waiting)
                {
                    _timer = 0f;
                    _elapsed = 0f;
                }
                yield return new WaitForFixedUpdate();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                _speed = 0f;
            }
        }
    }
}
