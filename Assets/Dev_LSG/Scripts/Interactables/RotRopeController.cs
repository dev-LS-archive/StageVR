using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Obi;

namespace Dev_LSG.Scripts.Interactables
{
    public class RotRopeController : MonoBehaviour
    {
        ObiRopeCursor _cursor;
        ObiRope _rope;
        public float minLength = 6.5f;
        public float maxLength = 9f;
        public float ropeSpeed = 1f;

        public int eventNum;
        public UnityEvent[] maxLengthEvent;
        public Rigidbody hook;

        // Use this for initialization
        void Start () {
            _cursor = GetComponentInChildren<ObiRopeCursor>();
            _rope = _cursor.GetComponent<ObiRope>();
        }
	
        // Update is called once per frame
        void Update () {
            if (Input.GetKey(KeyCode.W)){
                if (_rope.restLength > minLength)
                    _cursor.ChangeLength(_rope.restLength - ropeSpeed * Time.deltaTime);
            }
        
            if (Input.GetKey(KeyCode.S)){
                _cursor.ChangeLength(_rope.restLength + ropeSpeed * Time.deltaTime);
            }
        }

        public void CallLonger(int num)
        {
            eventNum = num;
            StartCoroutine(Longerer());
        }

        public void ResetLength()
        {
            _cursor.ChangeLength(minLength);
        }
        private IEnumerator Longerer()
        {
            while (_rope.restLength < maxLength)
            {
                hook.AddRelativeForce(new Vector3(0, 20, 400));
                _cursor.ChangeLength(_rope.restLength + ropeSpeed * Time.deltaTime);
                yield return null;
                print(_rope.restLength);
            }
            maxLengthEvent[eventNum].Invoke();
        }
    }
}
