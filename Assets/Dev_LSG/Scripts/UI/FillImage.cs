using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Dev_LSG.Scripts.UI
{
    public class FillImage : MonoBehaviour
    {
        public Image cooldown;
        public bool coolingDown;
        public float waitTime = 3.0f;
        public UnityEvent fullFillFunctions;

        public void ActFill()
        {
            coolingDown = true;
            StartCoroutine(Filling());
            //print("Act");
        }

        public void StopFill()
        {
            coolingDown = false;
            cooldown.fillAmount = 0;
            //print("Stop");
        }

        IEnumerator Filling()
        {
            while (coolingDown)
            {
                //Reduce fill amount over 30 seconds
                cooldown.fillAmount += 1.0f/waitTime * Time.deltaTime;
                yield return null;

                if (Math.Abs(Math.Abs(cooldown.fillAmount) - 1) == 0)
                {
                    fullFillFunctions.Invoke();
                    break;
                }
            }
        }
    }
}
