using UnityEngine;
using UnityEngine.SceneManagement;


namespace Dev_LSG.Scripts.Player
{
        public class PlayerRecenter : MonoBehaviour
        {
                [SerializeField] private Transform resetTransform;
                [SerializeField] private GameObject player;
                [SerializeField] private Camera playerHead;
                [SerializeField] private HVRHexaBodyInputs bodyInputs;
                
                public KeyCode recenter = KeyCode.Space;
                [SerializeField] private Transform firstPos;

                [SerializeField] private bool isFirstReset = false;

                public bool notResetEnable;
                //[SerializeField] private int isForward;

                void OnEnable()
                {
                        SceneManager.sceneLoaded += OnSceneLoaded;
                        // isForward = PlayerPrefs.GetInt("isForward", 0);
                        // if (isForward == 1)
                        // {
                        //         //print("reset");
                        //         Invoke(nameof(ResetPosition), 0.3f);
                        // }

                        if (!notResetEnable)
                        {
                                Invoke(
                                        SceneManager.GetActiveScene().name is "MainDirectionSelect" or "MainMenu"
                                                or "SelectMenu"
                                                ? nameof(ResetPosition)
                                                : nameof(FirstResetPosition), 0.3f);
                        }
                }

                public void SetDirection(int value)
                {
                        print(value);
                        PlayerPrefs.SetInt("isForward", value);
                        ResetPosition();
                }
                
                void OnSceneLoaded(Scene scene, LoadSceneMode mode)
                {
                        if (scene.name is "MainDirectionSelect" or "MainMenu" or "SelectMenu")
                        {
                                //print("Menu");
                                ResetPosition();
                        }
                        else
                        {
                                //print("First");
                                FirstResetPosition();
                        }

                        if (bodyInputs != null) 
                                bodyInputs.RecalibratePressed = true;
                }
                void OnDisable()
                {
                        //Debug.Log("OnDisable");
                        SceneManager.sceneLoaded -= OnSceneLoaded;
                }


                [ContextMenu("Reset Position")]
                public void ResetPosition()
                {
                        var rotationAngleY = resetTransform.transform.rotation.eulerAngles.y -
                                             playerHead.transform.rotation.eulerAngles.y;
                        player.transform.Rotate(0, rotationAngleY, 0);

                        var distanceDiff = resetTransform.position - playerHead.transform.position;
                        player.transform.position += distanceDiff;
                }

                public void FirstResetPosition()
                {
                        var rotationAngleY = firstPos.transform.rotation.eulerAngles.y -
                                             playerHead.transform.rotation.eulerAngles.y;
                        player.transform.Rotate(0, rotationAngleY, 0);

                        var distanceDiff = firstPos.position - playerHead.transform.position;
                        player.transform.position += distanceDiff;
                }

                public void NotFirstReset()
                {
                        isFirstReset = false;
                }

                public void NotResetEnable(bool enable)
                {
                        notResetEnable = enable;
                }
                private void Update()
                {
                        if (Input.GetKeyDown(recenter))
                        {
                                if (!isFirstReset)
                                {
                                        ResetPosition();
                                }
                                else
                                {
                                        FirstResetPosition();
                                }
                        }
                }
        }
}