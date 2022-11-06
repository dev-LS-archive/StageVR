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
        private float _rotSpeed;
        
        public UnityEvent[] endEvent;

        public bool brake;
        public Transform audioPos;
        public AudioSource brakeSound;
        public bool rotFast = false;
        public bool delayCall = false;
        public float delayDistance = 15f;

        public bool canRot = false;

        public void RotFastSet(bool value)
        {
            rotFast = value;
        }
        public void SpeedSet(float value)
        {
            speed = value;
        }
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

        public void SetDelayCall(bool value)
        {
            delayCall = value;
        }

        public void SetTransform()
        {
            end[order].position = transform.position;
        }

        public void Call_wait(float second)
        {
            StartCoroutine(Call_Wait(second));
        }
        IEnumerator Call_Wait(float second)
        {
            yield return new WaitForSeconds(second);
            CallMove();
        }
        IEnumerator Moving()
        {
            var distance = Vector3.Distance(_target, Rigidbody.position);
            var rotDiff = end[order].rotation.y - transform.rotation.y;
            if (rotFast == false)
            {
                _rotSpeed = 1 / distance;
            }
            else
            {
                _rotSpeed = 5;
            }
            //_rotSpeed = Mathf.Abs(rotDiff / distance);
            //print(_rotSpeed);
            while (!_waiting)
            {
                _speed = Mathf.Lerp(0, speed, _elapsed / timeToMaxSpeed);

                if (canRot == true)
                {
                    Rigidbody.rotation = Quaternion.Lerp(Rigidbody.rotation, end[order].rotation,
                        _rotSpeed * Time.deltaTime);
                }
                Rigidbody.MovePosition(Vector3.MoveTowards(Rigidbody.position, _target, _speed * Time.deltaTime));

                if (brake)
                {
                    if ((audioPos.position - Rigidbody.position).magnitude < 5)
                    {
                        //print("Audio");
                        brakeSound.Play();
                    }
                }

                float timing;
                timing = delayCall == true ? delayDistance : 0.01f;
                
                if ((_target - Rigidbody.position).magnitude < timing)
                    
                {
                    _speed = 0f;
                    if (canEvent)
                    {
                        if (order < endEvent.Length)
                            endEvent[order].Invoke();
                    }
                        
                    //print("Event");
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
