using UnityEngine;
using Obi;

namespace Dev_LSG.Scripts.Interactables
{
    public class RotRopeController : MonoBehaviour
    {
        ObiRopeCursor _cursor;
        ObiRope _rope;
        public float minLength = 6.5f;
        public float ropeSpeed = 1f;

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

            if (Input.GetKey(KeyCode.A)){
                transform.Rotate(0,Time.deltaTime*15f,0);
            }

            if (Input.GetKey(KeyCode.D)){
                transform.Rotate(0,-Time.deltaTime*15f,0);
            }
        }
    }
}
