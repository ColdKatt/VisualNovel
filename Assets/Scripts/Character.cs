using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// A class containing character data
/// </summary>
public class Character : MonoBehaviour
{
    private CharacterData _data;

    private string _currentSpriteName;

    private Image _image;

    private AsyncOperationHandle<Sprite> _currentSpriteHandle;

    private RectTransform _rectTransform;

    public void Init(string name, string clothes, string emote, string extra, string pose)
    {
        if (_data == null)
        {
            _data = new CharacterData(name, clothes, emote, extra, pose);
        }
        else
        {
            _data.Name = name;
            _data.Clothes = clothes;
            _data.Emote = emote;
            _data.Extra = extra;
            _data.Pose = pose;
        }

        _currentSpriteName = _data.Name + _data.Clothes + _data.Emote + _data.Extra + _data.Pose;

        _currentSpriteHandle = Addressables.LoadAssetAsync<Sprite>(_currentSpriteName);
        _currentSpriteHandle.Completed += ChangeSprite;
    }

    public string[] GetCharacterData()
    {
        return new string[] { _data.Name, _data.Clothes, _data.Emote, _data.Extra, _data.Pose };
    }

    public void ChangeSprite(AsyncOperationHandle<Sprite> sprite)
    {
        if (sprite.Status == AsyncOperationStatus.Succeeded)
        {
            _image.sprite = sprite.Result;
            _image.color = new Color(1, 1, 1, 255);
        }
    }

    public void MoveSprite(Vector3 shift)
    {
        _rectTransform.localPosition += shift;
    }

    public void SetPosition(Vector3 position)
    {
        _rectTransform.localPosition = new Vector3(position.x, _rectTransform.localPosition.y, _rectTransform.localPosition.z);
    }

    public Vector3 GetPosition()
    {
        return _rectTransform.localPosition;
    }

    private void Awake()
    {
        _image = gameObject.GetComponent<Image>();
        _image.color = new Color(255, 255, 255, 0);
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        _currentSpriteHandle.Completed -= ChangeSprite;
    }
}
