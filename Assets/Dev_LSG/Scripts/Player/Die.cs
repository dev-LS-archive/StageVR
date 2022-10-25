using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Dev_LSG.Scripts.Player
{
    public class Die : MonoBehaviour
    {
        public float waitTime = 10.0f;
        public Volume volume;
        private bool _fading = false;
        
        public UnityEvent fullFillFunctions;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Truck"))
            {
                //print("truck");
                if (_fading == false)
                {
                    _fading = true;
                    //StartCoroutine(Filling());
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Truck"))
            {
                //print("truck");
                if (_fading == false)
                {
                    _fading = true;
                    //StartCoroutine(Filling());
                }
            }
        }

        public void CallDie()
        {
            _fading = true;
            StartCoroutine(Filling());
        }

        IEnumerator Filling()
        {
            while (_fading)
            {
                //Reduce fill amount over 30 seconds
                volume.weight += 1.0f/waitTime * Time.deltaTime;
                yield return null;

                if (volume.weight >= 1)
                {
                    volume.weight = 1;
                    fullFillFunctions.Invoke();
                    break;
                }
            }
            yield return null;
        }
    }
}
