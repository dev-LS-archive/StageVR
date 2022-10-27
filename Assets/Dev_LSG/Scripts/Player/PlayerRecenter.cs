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
                
                public KeyCode Recenter = KeyCode.Space;
                [SerializeField] private Transform firstPos;

                void OnEnable()
                {
                        SceneManager.sceneLoaded += OnSceneLoaded;
                }
                
                void OnSceneLoaded(Scene scene, LoadSceneMode mode)
                {
                        if (scene.name == "MainMenu"||scene.name == "SelectMenu")
                        {
                                //print("Menu");
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
                        if (Input.GetKeyDown(Recenter))
                        {
                                ResetPosition();
                        }
                }
        }
}
