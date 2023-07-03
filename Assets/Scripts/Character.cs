using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    public CharacterData Data;

    private string _currentSpriteName;

    private Image _image;

    public AsyncOperationHandle<Sprite> CurrentSprite;

    void Awake()
    {
        _image = gameObject.GetComponent<Image>();
        _image.color = new Color(255, 255, 255, 0);
    }

    public void Init(string name, string clothes, string emote, string extra, string pose)
    {
        if (Data == null)
        {
            Data = ScriptableObject.CreateInstance<CharacterData>();
        }
        Data.Name = name;
        Data.Clothes = clothes;
        Data.Emote = emote;
        Data.Extra = extra;
        Data.Pose = pose;
        _currentSpriteName = Data.Name + Data.Clothes + Data.Emote + Data.Extra + Data.Pose;

        CurrentSprite = Addressables.LoadAssetAsync<Sprite>(_currentSpriteName);
        CurrentSprite.Completed += ChangeSprite;
        Debug.Log(_currentSpriteName);
    }

    public void ChangeSprite(AsyncOperationHandle<Sprite> sprite)
    {
        Debug.Log("IC");
        if (sprite.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log(_image);
            _image.sprite = sprite.Result;
            _image.color = new Color(1, 1, 1, 255);
        }
    }

    private void OnDestroy()
    {
        CurrentSprite.Completed -= ChangeSprite;
    }
}
