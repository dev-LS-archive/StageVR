using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Player
{
    public class PlayerBackwardEvent : MonoBehaviour
    {
        public UnityEvent lookBackEvent;
        [SerializeField] private bool check = false;

        private void Start()
        {
            check = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (check == false)
            {
                print(transform.eulerAngles.y);
                if (transform.eulerAngles.y is > 160 and < 200)
                {
                    lookBackEvent.Invoke();
                    check = true;
                }
            }
        }
    }
}
