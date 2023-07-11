using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// A class for managing commands from an xml scenario file
/// </summary>
public class Commands : MonoBehaviour
{
    public static Dictionary<string, string> s_Commands;

    public static Dictionary<string, Character> s_Characters;

    public static bool s_IsChooseEventActive = false;
    public static bool s_IsEndReached = false;

    public SceneData SceneData;

    public TextAppear TextAppear;

    [SerializeField] private Image _backImage;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _characterName;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Transform _chooseWindow;
    [SerializeField] private Transform _spriteHandle;
    [SerializeField] private TMP_Text _parameterInfoText;
    [SerializeField] private GameObject _endPanel;

    private Button _timeoutButton;
    private Coroutine _timeCoroutine;

    private AsyncOperationHandle<Sprite> _backHandle;
    private AsyncOperationHandle<AudioClip> _musicHandle;
    private AsyncOperationHandle<GameObject> _buttonHandle;
    private AsyncOperationHandle<GameObject> _characterTemplateHandle;

    public void SetBack(string[] backName)
    {
        _backHandle = Addressables.LoadAssetAsync<Sprite>(backName[0]);
        _backHandle.Completed += ChangeBackground;

        SceneData.CurrentBack = backName[0];
    }

    public void SetMusic(string[] musicName)
    {
        _musicHandle = Addressables.LoadAssetAsync<AudioClip>(musicName[0]);
        _musicHandle.Completed += ChangeMusic;

        SceneData.CurrentMusic = musicName[0];
    }

    public void ChangeBackground(AsyncOperationHandle<Sprite> back)
    {
        if (back.Status == AsyncOperationStatus.Succeeded)
        {
            _backImage.sprite = back.Result;
        }
    }

    public void ChangeMusic(AsyncOperationHandle<AudioClip> music)
    {
        if (music.Status == AsyncOperationStatus.Succeeded)
        {
            _audioSource.clip = music.Result;
            _audioSource.Play();
        }
    }

    /// <summary>
    /// Creates a choose event and generates options
    /// </summary>
    /// <param name="chooses">An array of elements having the structure: optionLabel;lineNumber</param>
    public void Choose(string[] chooses)
    {
        s_IsChooseEventActive = true;
        for (var i = 0; i < chooses.Length; i++)
        {
            if (_buttonHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var chooseParams = chooses[i].Split(';');
                var chooseButton = Instantiate(_buttonHandle.Result, _chooseWindow);

                var sign = i % 2 == 0 ? 1 : -1;
                var delta = i % 2 == 0 ? i / 2 : (i / 2) + 1;
                chooseButton.GetComponent<RectTransform>().localPosition = new Vector2(0, delta * 45 * sign);

                chooseButton.GetComponentInChildren<TMP_Text>().text = chooseParams[0];
                var chooseButtonComp = chooseButton.GetComponent<Button>();

                chooseButtonComp.onClick.AddListener(() => History.CreateElement(chooseParams[0], TextAppear.CurrentLine));
                chooseButtonComp.onClick.AddListener(() => TextAppear.ChangeCurrentLine(int.Parse(chooseParams[1]), true));
                chooseButtonComp.onClick.AddListener(() => CancelChoose());

                if (i == chooses.Length - 1)
                {
                    _timeoutButton = chooseButtonComp;
                }
            }
        }
    }

    /// <summary>
    /// Changes the parameter by this value. Creates a new parameter if there is none in the dictionary <see cref="GameParameters.s_Parameters"/>
    /// </summary>
    /// <param name="data">An array of two elements: 0 - Parameter name, 1 - Shift</param>
    public void ChangeParameter(string[] data)
    {
        GameParameters.ChangeParameter(data[0], int.Parse(data[1]));
    }

    public void DestroyChooseButtons()
    {
        foreach (Transform child in _chooseWindow)
        {
            child.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Creates a new character that is not yet in the dictionary <see cref="s_Characters"/>
    /// </summary>
    /// <param name="characterData">An array of five elements: name,clothes,emote,extra,pose</param>
    public void CreateCharacter(string[] characterData)
    {
        var name = characterData[0];
        var clothes = characterData[1];
        var emote = characterData[2];
        var extra = characterData[3];
        var pose = characterData[4];

        if (_characterTemplateHandle.Status == AsyncOperationStatus.Succeeded && !s_Characters.ContainsKey(name))
        {
            var character = Instantiate(_characterTemplateHandle.Result, _spriteHandle);
            var characterComponent = character.GetComponent<Character>();
            characterComponent.Init(name, clothes, emote, extra, pose);
            s_Characters.Add(name, characterComponent);
        }
    }

    /// <summary>
    /// Modifies the data of a character that exists in the dictionary <see cref="s_Characters"/>
    /// </summary>
    /// <param name="spriteData">An array of five elements: name,clothes,emote,extra,pose</param>
    public void ChangeSprite(string[] spriteData)
    {
        var name = spriteData[0];
        var clothes = spriteData[1];
        var emote = spriteData[2];
        var extra = spriteData[3];
        var pose = spriteData[4];

        if (s_Characters.ContainsKey(name))
        {
            s_Characters[name].Init(name, clothes, emote, extra, pose);
        }
    }

    /// <summary>
    /// Deletes an existing character in the dictionary
    /// </summary>
    /// <param name="name">Character name</param>
    public void DeleteSprite(string[] name)
    {
        if (s_Characters.ContainsKey(name[0]))
        {
            var character = s_Characters[name[0]];
            s_Characters.Remove(name[0]);
            Destroy(character.gameObject);
        }
    }

    public void GoLine(string[] lineNum)
    {
        TextAppear.ChangeCurrentLine(int.Parse(lineNum[0]), false);
    }

    /// <summary>
    /// Compares the parameter value with the given value. 
    /// If the parameter is greater than or equal to the value being compared, it switches to the transmitted text line
    /// </summary>
    /// <param name="compareData">An array of three elements: 0 - Param name, 1 - Compare value, 2 - Line number</param>
    public void Compare(string[] compareData)
    {
        var compareParameter = GameParameters.s_Parameters[compareData[0]];
        var compareValue = int.Parse(compareData[1]);
        var lineNum = compareData[2];

        if (compareParameter >= compareValue)
        {
            GoLine(new string[] { lineNum });
        }
    }

    public IEnumerator CountDown(float time)
    {
        _timerText.alpha = 1.0f;

        while(time > 0.01f)
        {
            time -= Time.deltaTime;
            _timerText.text = time.ToString("0.00");
            yield return null;
        }

        _timeoutButton.onClick.Invoke();
        TextAppear.ShowNextTextLine();
        _timeoutButton = null;
    }

    public void SetTimer(string[] time)
    {
        _timeCoroutine = StartCoroutine(CountDown(float.Parse(time[0])));
    }

    public void StopTimer()
    {
        if (_timeCoroutine != null)
        {
            StopCoroutine(_timeCoroutine);
            _timeCoroutine = null;
        }
    }

    public void SetName(string[] name)
    {
        _characterName.text = name[0];
    }

    /// <summary>
    /// Moves the character by a given value.
    /// </summary>
    /// <param name="moveData">An array of two elements: 0 - Character name, 1 - Shift</param>
    public void MoveCharacter(string[] moveData)
    {
        var name = moveData[0];
        var shift = float.Parse(moveData[1]);

        if (s_Characters.ContainsKey(name))
        {
            s_Characters[name].MoveSprite(Vector3.right * shift);
        }
    }

    /// <summary>
    /// Enables the ending panel
    /// </summary>
    /// <param name="end"></param>
    public void ShowEnd(string[] end)
    {
        s_IsEndReached = true;

        foreach (var key in GameParameters.s_Parameters.Keys)
        {
            _parameterInfoText.text += $"{key} : {GameParameters.s_Parameters[key]}";
        }

        _endPanel.SetActive(true);
    }

    public void CancelChoose()
    {
        s_IsChooseEventActive = false;
        _timerText.alpha = 0f;
        StopTimer();
        DestroyChooseButtons();
    }

    private void Start()
    {
        _timerText.alpha = 0f;
        s_Commands = new Dictionary<string, string>();
        s_Characters = new Dictionary<string, Character>();
        s_Commands.Add("back", "SetBack");
        s_Commands.Add("choose", "Choose");
        s_Commands.Add("music", "SetMusic");
        s_Commands.Add("init", "CreateCharacter");
        s_Commands.Add("change", "ChangeSprite");
        s_Commands.Add("delete", "DeleteSprite");
        s_Commands.Add("go", "GoLine");
        s_Commands.Add("compare", "Compare");
        s_Commands.Add("time", "SetTimer");
        s_Commands.Add("speaker", "SetName");
        s_Commands.Add("move", "MoveCharacter");
        s_Commands.Add("end", "ShowEnd");
        s_Commands.Add("shift", "ChangeParameter");
        _buttonHandle = Addressables.LoadAssetAsync<GameObject>("ChooseButton");
        _characterTemplateHandle = Addressables.LoadAssetAsync<GameObject>("Character");
        SceneData = new SceneData();
    }
}
