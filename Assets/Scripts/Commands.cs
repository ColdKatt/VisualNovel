using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class Commands : MonoBehaviour
{
    public static Dictionary<string, Delegate> s_Commands;

    public static Dictionary<string, Character> s_Characters;

    public static bool s_isChooseEventActive = false;

    private Button _timeoutButton;
    private Coroutine _timeCoroutine;
    public TMP_Text TimerText;

    public TMP_Text CharacterName;

    // SetBack;backName|
    public AsyncOperationHandle<Sprite> back;
    public Image backImage;

    public delegate void OnBackSet(params string[] background);
    public OnBackSet onBackSet;

    // SetMusic;musicName|
    public AsyncOperationHandle<AudioClip> music;
    public AudioSource audioSource;

    public delegate void OnMusicSet(params string[] music);
    public OnMusicSet onMusicSet;

    // Choose;chooseText,lineNum;chooseText2,lineNum2;...|
    public AsyncOperationHandle<GameObject> button;
    public delegate void OnChoose(params string[] chooses);
    public OnChoose onChoose;
    public Transform chooseWindow;

    //SetTimer;time|
    public delegate void OnTimerSet(params string[] time);
    public OnTimerSet onTimerSet;

    // ChangeParameter;parameterName;shift|
    public delegate void OnParameterChange(params string[] parameterData);
    public OnParameterChange onParameterChange;

    // CreateCharacter;name;clothes;emote;extra;pose|
    public AsyncOperationHandle<GameObject> characterTemplate;
    public delegate void OnCreateCharacter(params string[] characterData);
    public OnCreateCharacter onCreateCharacter;
    public Transform SpriteHandle;

    //ChangeSprite;name;clothes;emote;extra;pose|
    public delegate void OnChangeSprite(params string[] spriteData);
    public OnChangeSprite onChangeSprite;

    //DeleteSprite;name|
    public delegate void OnSpriteDelete(params string[] spriteName);
    public OnSpriteDelete onSpriteDelete;

    //GoToLine;lineNum|
    public delegate void OnLineChange(params string[] lineNum);
    public OnLineChange onLineChange;

    //Compare;parameterName;compareValue;lineNum|
    public delegate void OnCompare(params string[] compareData);
    public OnCompare onCompare;

    //SetName;name|
    public delegate void OnNameSet(params string[] name);
    public OnNameSet onNameSet;

    //MoveCharacter;name;shift|
    public delegate void OnMove(params string[] moveData);
    public OnMove onMove;

    //ShowEnd;|
    public TMP_Text ParameterInfoText;
    public GameObject EndPanel;
    public delegate void OnEnd(params string[] end);
    public OnEnd onEnd;

    void Start()
    {
        TimerText.alpha = 0f;
        s_Commands = new Dictionary<string, Delegate>();
        s_Characters = new Dictionary<string, Character>();
        onBackSet += SetBack;
        onChoose += Choose;
        onMusicSet += SetMusic;
        onCreateCharacter += CreateCharacter;
        onChangeSprite += ChangeSprite;
        onSpriteDelete += DeleteSprite;
        onLineChange += GoLine;
        onCompare += Compare;
        onTimerSet += SetTimer;
        onNameSet += SetName;
        onMove += MoveCharacter;
        onEnd += ShowEnd;
        s_Commands.Add("SetBack", onBackSet);
        s_Commands.Add("Choose", onChoose);
        s_Commands.Add("SetMusic", onMusicSet);
        s_Commands.Add("CreateCharacter", onCreateCharacter);
        s_Commands.Add("ChangeSprite", onChangeSprite);
        s_Commands.Add("DeleteSprite", onSpriteDelete);
        s_Commands.Add("GoLine", onLineChange);
        s_Commands.Add("Compare", onCompare);
        s_Commands.Add("SetTimer", onTimerSet);
        s_Commands.Add("SetName", onNameSet);
        s_Commands.Add("MoveCharacter", onMove);
        s_Commands.Add("ShowEnd", onEnd);
        button = Addressables.LoadAssetAsync<GameObject>("ChooseButton");
        characterTemplate = Addressables.LoadAssetAsync<GameObject>("Character");
    }

    public void SetBack(params string[] backName)
    {
        back = Addressables.LoadAssetAsync<Sprite>(backName[0]);
        back.Completed += ChangeBackground;
    }

    public void SetMusic(params string[] musicName)
    {
        music = Addressables.LoadAssetAsync<AudioClip>(musicName[0]);
        music.Completed += ChangeMusic;
    }

    public void ChangeBackground(AsyncOperationHandle<Sprite> back)
    {
        if (back.Status == AsyncOperationStatus.Succeeded)
        {
            backImage.sprite = back.Result;
        }
    }

    public void ChangeMusic(AsyncOperationHandle<AudioClip> music)
    {
        if (music.Status == AsyncOperationStatus.Succeeded)
        {
            audioSource.clip = music.Result;
            audioSource.Play();
        }
    }

    public void Choose(params string[] chooses)
    {
        s_isChooseEventActive = true;
        Debug.Log("Enter");
        for (var i = 0; i < chooses.Length; i++)
        {
            if (button.Status == AsyncOperationStatus.Succeeded)
            {
                var chooseParams = chooses[i].Split(',');
                var chooseButton = Instantiate(button.Result, chooseWindow);
                var sign = i % 2 == 0 ? 1 : -1;
                var delta = i % 2 == 0 ? i / 2 : (i / 2) + 1;
                Debug.Log(delta);
                chooseButton.GetComponent<RectTransform>().localPosition = new Vector2(0, delta * 45 * sign);
                chooseButton.GetComponentInChildren<TMP_Text>().text = chooseParams[0];
                var chooseButtonComp = chooseButton.GetComponent<Button>();
                chooseButtonComp.onClick.AddListener(() => GoLine(chooseParams[1]));
                chooseButtonComp.onClick.AddListener(() => History.CreateElement(chooseParams[0], TextAppear.currentLine));
                chooseButtonComp.onClick.AddListener(() => s_isChooseEventActive = false);
                chooseButtonComp.onClick.AddListener(() => TimerText.alpha = 0f);
                chooseButtonComp.onClick.AddListener(() => StopTimer());
                chooseButtonComp.onClick.AddListener(() => DestroyChooseButtons());
                if (i == chooses.Length - 1)
                {
                    _timeoutButton = chooseButtonComp;
                }
            }
        }
    }

    public void ChangeParameter(params string[] parameterData)
    {
        GameParameters.ChangeParameter(parameterData[0], int.Parse(parameterData[1]));
    }

    public void DestroyChooseButtons()
    {
        foreach (Transform child in chooseWindow)
        {
            child.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(child.gameObject);
        }
    }

    public void CreateCharacter(params string[] characterData)
    {
        var name = characterData[0];
        var clothes = characterData[1];
        var emote = characterData[2];
        var extra = characterData[3];
        var pose = characterData[4];
        if (characterTemplate.Status == AsyncOperationStatus.Succeeded && !s_Characters.ContainsKey(name))
        {
            var character = Instantiate(characterTemplate.Result, SpriteHandle);
            var characterComponent = character.GetComponent<Character>();
            characterComponent.Init(name, clothes, emote, extra, pose);
            s_Characters.Add(name, characterComponent);
        }
    }

    public void ChangeSprite(params string[] spriteData)
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

    public void DeleteSprite(params string[] name)
    {
        if (s_Characters.ContainsKey(name[0]))
        {
            var character = s_Characters[name[0]];
            s_Characters.Remove(name[0]);
            Destroy(character.gameObject);
        }
    }

    public void GoLine(params string[] lineNum)
    {
        TextAppear.ChangeCurrentLine(int.Parse(lineNum[0]));
    }

    public void Compare(params string[] compareData)
    {
        var compareParameter = GameParameters.s_Parameters[compareData[0]];
        var compareValue = int.Parse(compareData[1]);
        var lineNum = compareData[2];
        if (compareParameter >= compareValue)
        {
            GoLine(lineNum);
        }
    }

    //Timer
    public IEnumerator CountDown(float time)
    {
        TimerText.alpha = 1.0f;
        while(time > 0.01f)
        {
            time -= Time.deltaTime;
            TimerText.text = time.ToString("0.00");
            yield return null;
        }
        _timeoutButton.onClick.Invoke();
        _timeoutButton = null;
    }

    public void SetTimer(params string[] time)
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

    public void SetName(params string[] name)
    {
        CharacterName.text = name[0];
    }

    // Should Add DOTween?
    public void MoveCharacter(params string[] moveData)
    {
        var name = moveData[0];
        var shift = float.Parse(moveData[1]);
        if (s_Characters.ContainsKey(name))
        {
            s_Characters[name].GetComponent<RectTransform>().localPosition += Vector3.right * shift;
        }
    }

    public void ShowEnd(params string[] end)
    {
        foreach (var key in GameParameters.s_Parameters.Keys)
        {
            ParameterInfoText.text += $"{key} : {GameParameters.s_Parameters[key]}";
        }
        EndPanel.SetActive(true);
    }

    private void OnDisable()
    {
        onBackSet -= SetBack;
        back.Completed -= ChangeBackground; // Has Error on Disabling
    }
}
