using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using HurricaneVR.Framework.ControllerInput;

namespace Dev_LSG.Scripts.Event
{
    public class CollEventTag : MonoBehaviour
    {
        public string tagStr;
        public UnityEvent fullFillFunctions;
        public float waitTime = 3.0f;
        private float _fillAmount;
        
        public bool coolingDown;
        public bool isTrigger = false;
        public AudioSource fillSound;

        private void OnEnable()
        {
            AddListen();
        }

        private void OnDisable()
        {
            RemoveListen();
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(tagStr))
            {
                isTrigger = true;
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (other.gameObject.CompareTag(tagStr))
            {
                isTrigger = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(tagStr))
            {
                isTrigger = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag(tagStr))
            {
                isTrigger = false;
            }
        }

        private void AddListen()
        {
            //print(("enable"));
            HVRControllerEvents.Instance.RightTriggerActivated.AddListener(ActFill);
            HVRControllerEvents.Instance.RightTriggerDeactivated.AddListener(StopFill);
        }

        private void RemoveListen()
        {
            //print(("disable"));
            HVRControllerEvents.Instance.RightTriggerActivated.RemoveListener(ActFill);
            HVRControllerEvents.Instance.RightTriggerDeactivated.RemoveListener(StopFill);
        }

        private void ActFill()
        {
            coolingDown = true;
            StartCoroutine(Filling());
            //print("Act");
        }

        private void StopFill()
        {
            coolingDown = false;
            _fillAmount = 0;
            //print("Stop");
        }
        
        IEnumerator Filling()
        {
            if (isTrigger)
            {
                while (coolingDown)
                {
                    fillSound.Play();
                    //Reduce fill amount
                    _fillAmount += 1.0f/waitTime * Time.deltaTime;
                    yield return null;

                    //print(Math.Abs(Math.Abs(_fillAmount)));

                    if (Math.Abs(_fillAmount) - 1 >= 0)
                    {
                        fillSound.Stop();
                        fullFillFunctions.Invoke();
                        break;
                    }
                }
                fillSound.Stop();
            }
        }
        [ContextMenu("InvokeFunction")]
        void InvokeFunction()
        {
            fullFillFunctions.Invoke();
        }
    }
}
