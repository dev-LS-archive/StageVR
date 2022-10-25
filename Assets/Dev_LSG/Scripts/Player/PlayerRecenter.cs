using UnityEngine;



namespace Dev_LSG.Scripts.Player
{
        public class PlayerRecenter : MonoBehaviour
        {
                [SerializeField] private Transform resetTransform;
                [SerializeField] private GameObject player;
                [SerializeField] private Camera playerHead;
                
                public KeyCode Recenter = KeyCode.Space;
                

                [ContextMenu("Reset Position")]
                public void ResetPosition()
                {
                        var rotationAngleY = resetTransform.transform.rotation.eulerAngles.y -
                                             playerHead.transform.rotation.eulerAngles.y;
                        player.transform.Rotate(0, rotationAngleY, 0);

                        var distanceDiff = resetTransform.position - playerHead.transform.position;
                        player.transform.position += distanceDiff;
                }

                private void Update()
                {
                        if (Input.GetKeyDown(Recenter))
                        {
                                ResetPosition();
                        }
                }
        }
}
