using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Dev_LSG.Scripts.Manager
{
    public class BGMPlayerFort : MonoBehaviour
    {
        public AudioSource bgmAudioSource;
        public AudioClip fortBGM;
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
            print(scene.name);

            if (scene.name == "SelectMenu_Sea")
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = null;
                bgmAudioSource.loop = false;
                bgmAudioSource.volume = 1f;
                // if (scene.name == "MainMenu")
                // {
                //     StartCoroutine(DelayPlay(main));
                // }
                StartCoroutine(DelayPlay(select));
            }
            else
            {
                bgmAudioSource.loop = true;
                bgmAudioSource.volume = 0.2f;
                print(scene.name);
                Play(fortBGM);
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
