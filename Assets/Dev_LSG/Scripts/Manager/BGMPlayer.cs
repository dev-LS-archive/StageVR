using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev_LSG.Scripts.Manager
{
    public class BGMPlayer : MonoBehaviour
    {
        public AudioSource bgmAudioSource;
        public AudioClip roadBGM;
        public AudioClip construction;
        public AudioClip hammering;
        public AudioClip main;
        public AudioClip select;

        // called first
        void OnEnable()
        {
            //Debug.Log("OnEnable called");
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // called second
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bgmAudioSource.loop = true;
            bgmAudioSource.volume = 0.2f;
            if (scene.name == "RemovalofSafety_1" || scene.name == "RemovalofSafety_1_end")
            {
                Play(roadBGM);
            }
            else if (scene.name == "RemovalofSafety_2" || scene.name == "RemovalofSafety_2_end" ||
                     scene.name == "RemovalofSafety_2_end_1" || scene.name == "RemovalofSafety_2_end_2")
            {
                Play(roadBGM);
            }
            else if (scene.name == "ShoulderofSafety")
            {
                Play(roadBGM);
            }
            else
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = null;
                bgmAudioSource.loop = false;
                bgmAudioSource.volume = 1f;
                // if (scene.name == "MainMenu")
                // {
                //     StartCoroutine(DelayPlay(main));
                // }
                if (scene.name == "SelectMenu")
                {
                    StartCoroutine(DelayPlay(select));
                }
            }
            //Debug.Log("OnSceneLoaded: " + scene.name);
            //Debug.Log(mode);
        }

        IEnumerator DelayPlay(AudioClip clip)
        {
            yield return new WaitForSeconds(0.5f);
            Play(clip);
        }

        void Play(AudioClip clip)
        {
            bgmAudioSource.clip = clip;
            bgmAudioSource.Play();
        }
        // called when the game is terminated
        void OnDisable()
        {
            //Debug.Log("OnDisable");
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
