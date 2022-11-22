using System;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Dev_LSG.Scripts.Player
{
        public class PlayerRecenter : MonoBehaviour
        {
                [SerializeField] private Transform resetTransform;
                [SerializeField] private GameObject player;
                [SerializeField] private Camera playerHead;
                
                public KeyCode recenter = KeyCode.Space;
                [SerializeField] private Transform firstPos;
                [SerializeField] private int isForward;

                void OnEnable()
                {
                        SceneManager.sceneLoaded += OnSceneLoaded;
                        isForward = PlayerPrefs.GetInt("isForward", 0);
                        if (isForward == 1)
                        {
                                print("reset");
                                Invoke(nameof(ResetPosition), 0.3f);
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
                                print("Menu");
                                ResetPosition();
                        }
                        else
                        {
                                //print("First");
                                FirstResetPosition();
                        }
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
                
                private void Update()
                {
                        if (Input.GetKeyDown(recenter))
                        {
                                ResetPosition();
                        }
                }
        }
}
