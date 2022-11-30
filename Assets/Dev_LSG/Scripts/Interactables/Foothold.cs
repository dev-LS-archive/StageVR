using System.Collections;
using HexabodyVR.PlayerController;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Dev_LSG.Scripts.Interactables
{
    public class Foothold : MonoBehaviour
    {
        public float waitTime = 10.0f;
        public Volume volume;
        public Transform body;
        [SerializeField]
        private bool fading = false;
        
        [SerializeField]
        private bool dontFade = false;
        [SerializeField]
        private bool dontSnap = false;
        [SerializeField]
        private bool actOn = false;

        public UnityEvent fullFillFunctions;
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                //print("PlayerEnter");
                if (fading == false)
                {
                    //print("call");
                    fading = true;
                    StartCoroutine(Fade());
                }
            }
        }

        // private void OnTriggerExit(Collider other)
        // {
        //     if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        //     {
        //         //print("PlayerExit");
        //         volume.weight = 0;
        //         //fading = false;
        //     }
        // }

        public void CallUnfade()
        {
            StartCoroutine(UnFade());
        }

        IEnumerator Fade()
        {
            //print(1.0f/waitTime);
            while (fading)
            {
                if (dontFade)
                {
                    fullFillFunctions.Invoke();
                    if(!actOn)
                        gameObject.SetActive(false);
                    break;
                }
                //Reduce fill amount over 30 seconds
                volume.weight += 1.0f/waitTime * Time.deltaTime;
                yield return null;

                if (volume.weight >= 1)
                {
                    volume.weight = 1;
                    if (!dontSnap) 
                        body.GetComponent<HexaBodyPlayer4>().CallSnapTurn();
                    fullFillFunctions.Invoke();
                    StartCoroutine(UnFade());
                    break;
                }
            }
            yield return null;
        }

        IEnumerator UnFade()
        {
            while (fading)
            {
                //Reduce fill amount over 30 seconds
                volume.weight -= 1.0f/waitTime * Time.deltaTime;
                yield return null;

                if (volume.weight <= 0)
                {
                    volume.weight = 0;
                    if(!actOn)
                        gameObject.SetActive(false);
                    break;
                }
            }
            yield return null;
            if (volume.weight != 0)
            {
                StartCoroutine(UnFade());
            }
        }
    }
}
