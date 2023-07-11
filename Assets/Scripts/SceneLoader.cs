using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private AsyncOperationHandle<SceneInstance> _sceneHandle;

    public void LoadScene(string scene)
    {
        _sceneHandle = Addressables.LoadSceneAsync(scene);
    }
}
