using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Zenject;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Reflection;

public class TextAppear : MonoBehaviour
{
    public Commands Commands;
    public AsyncOperationHandle<TextAsset> Scenario;
    public TMP_Text WindowText;
    private string[] _textLines;
    private string _textLine = "—ъешь ж ещЄ этих м€гких французских булок, да выпей чаю...!";
    private string[] _textLineParams;
    private string[] _params;
    public static int currentLine = 0;

    public InputSystem input;

    private Coroutine m_coroutine;

    void Start()
    {
        Scenario = Addressables.LoadAssetAsync<TextAsset>("TestScenario"); // Scenario structure: MethodA;param1;param2...|MethodB;param1;param2;...|...|TextLine
        Scenario.Completed += GetScenario;
        Lean.Touch.LeanTouch.OnFingerTap += ShowNextTextLine;
    }

    //void Update()
    //{
    //    ShowNextTextLine();
    //}

    public IEnumerator PrintText()
    {
        Debug.Log($"Current line: {currentLine}");
        WindowText.text = "";
        _textLine = _textLines[currentLine];
        _textLineParams = _textLine.Split('|');

        for (var i = 0; i < _textLineParams.Length - 1; i++)
        {
            _params = _textLineParams[i].Split(';');
            var type = Type.GetType("Commands");
            var method = type.GetMethod(_params[0]);
            var parameters = _params.Skip(1).ToArray();
            method.Invoke(Commands, new object[] { parameters });
        }

        foreach (var letter in _textLineParams[_textLineParams.Length - 1])
        {
            Debug.Log(letter);
            WindowText.text += letter;
            yield return new WaitForSeconds(0.02f);
        }
        m_coroutine = null;
        currentLine++;
    }

    public void ShowNextTextLine(Lean.Touch.LeanFinger finger)
    {
        if (!Commands.s_isChooseEventActive)
        {
            ShowNextTextLine();
        }
    }

    public void ShowNextTextLine()
    {
        if (input.IsSwipedActionActived)
        {
            input.ReturnToDefault();
            return;
        }

        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
            m_coroutine = null;
            WindowText.text = _textLineParams[_textLineParams.Length - 1];
            currentLine++;
        }
        else
        {
            m_coroutine = StartCoroutine(PrintText());
        }
    }

    public void GetScenario(AsyncOperationHandle<TextAsset> scenario)
    {
        if (scenario.Status == AsyncOperationStatus.Succeeded)
        {
            _textLines = scenario.Result.text.Split('\n');
            foreach (var line in _textLines)
            {
                Debug.Log(line);
            }
            Debug.Log("TestScenario");
            Debug.Log(scenario.Result);
        }
    }

    public static void ChangeCurrentLine(int lineNum)
    {
        currentLine = lineNum;
    }
}
