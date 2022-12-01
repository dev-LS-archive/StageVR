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
        public UnityEvent[] minLengthEvent;
        public Rigidbody hook;

        // Use this for initialization
        void Start () {
            _cursor = GetComponentInChildren<ObiRopeCursor>();
            _rope = _cursor.GetComponent<ObiRope>();
        }
	
        // Update is called once per frame
        // void Update () {
        //     if (Input.GetKey(KeyCode.W)){
        //         if (_rope.restLength > minLength)
        //             _cursor.ChangeLength(_rope.restLength - ropeSpeed * Time.deltaTime);
        //     }
        //
        //     if (Input.GetKey(KeyCode.S)){
        //         _cursor.ChangeLength(_rope.restLength + ropeSpeed * Time.deltaTime);
        //     }
        // }

        public void CallLonger(int num)
        {
            eventNum = num;
            StartCoroutine(Longerer());
        }
        
        public void CallShorter(int num)
        {
            eventNum = num;
            StartCoroutine(Shorter());
        }

        public void ResetLength(int num)
        {
            _cursor.ChangeLength(minLength);
            minLengthEvent[num].Invoke();
        }
        private IEnumerator Longerer()
        {
            while (_rope.restLength < maxLength)
            {
                _cursor.ChangeLength(_rope.restLength + ropeSpeed * Time.deltaTime);
                yield return null;
                //print(_rope.restLength);
            }
            maxLengthEvent[eventNum].Invoke();
        }
        
        private IEnumerator Shorter()
        {
            while (_rope.restLength > minLength)
            {
                _cursor.ChangeLength(_rope.restLength - ropeSpeed * Time.deltaTime);
                yield return null;
                //print(_rope.restLength);
            }
            minLengthEvent[eventNum].Invoke();
        }
    }
}
