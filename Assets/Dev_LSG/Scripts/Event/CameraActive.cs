using UnityEngine;

namespace Dev_LSG.Scripts.Event
{
    public class CameraActive : MonoBehaviour
    {
        public GameObject cam;
        public Transform cameraPos;
        
        public void CamAct()
        {
            cam.transform.position = cameraPos.position;
            cam.transform.rotation = cameraPos.rotation;

            cam.gameObject.SetActive(cam.activeSelf != true);
        }
    }
}
