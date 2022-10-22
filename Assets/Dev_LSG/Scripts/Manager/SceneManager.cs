using System;
using System.Collections;
using Dev_LSG.Scripts.Core.Singletons;
using UnityEngine;

namespace Dev_LSG.Scripts.Manager
{
    public class SceneManager : Singleton<SceneManager>
    {

        public void SceneLoad(string sceneName)
        {
            StartCoroutine(LoadYourAsyncScene(sceneName));
        }
        
        IEnumerator LoadYourAsyncScene(string sceneName)
        {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }
}
