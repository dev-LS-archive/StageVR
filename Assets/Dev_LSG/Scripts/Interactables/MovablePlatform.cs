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
    
        void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();
            _target = end[order].position;
            _endTarget = true;
            _waiting = true;
        }

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
                Rigidbody.MovePosition(Vector3.MoveTowards(Rigidbody.position, _target, _speed * Time.deltaTime));

                if ((_target - Rigidbody.position).magnitude < .01)
                {
                    _speed = 0f;
                    if (canEvent)
                        endEvent[order].Invoke();
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
    }
}
