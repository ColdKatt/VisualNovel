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
using System.Xml.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// A class for managing appearing text
/// </summary>
public class TextAppear : MonoBehaviour
{
    public int CurrentLine = 0;

    [SerializeField] private InputSystem _input;

    [SerializeField] private Commands _commands;
    [SerializeField] private TMP_Text _windowText;

    [SerializeField] private TTS _tts;

    private AsyncOperationHandle<TextAsset> _scenarioHandle;
    private XElement[] _textLines;
    private XElement _textLine;

    private Coroutine _printTextCoroutine;

    public IEnumerator PrintText()
    {
        Debug.Log($"Current line: {CurrentLine}");
        _windowText.text = "";
        _textLine = _textLines[CurrentLine];

        var sceneData = _textLine.Element("sceneData");
        var sprites = _textLine.Element("sprites");
        var gameParams = _textLine.Element("params");
        var text = _textLine.Element("text");

        ProcessAttributes(_textLine);
        ProcessAttributes(sceneData);
        ProcessAttributes(sprites, "sprite");
        ProcessAttributes(gameParams, "param");
        ProcessAttributes(text);

        if (_input.IsTTSEnabled)
        {
            _tts.Speak(text.Value);
        }

        foreach (var letter in text.Value)
        {
            _windowText.text += letter;
            yield return new WaitForSeconds(0.02f);
        }

        _printTextCoroutine = null;
    }

    public void ShowNextTextLine(Lean.Touch.LeanFinger finger)
    {
        _input.StopSkip();
        
        ShowNextTextLine();
    }

    public void ShowNextTextLine()
    {
        if (_input.IsSwipedActionActived && !_input.IsSkipping)
        {
            _input.ReturnToDefault();
            return;
        }

        if (Commands.s_IsEndReached || Commands.s_IsChooseEventActive)
        {
            _input.StopSkip();
            return;
        }

        if (_printTextCoroutine != null)
        {
            StopCoroutine(_printTextCoroutine);
            _printTextCoroutine = null;
            _windowText.text = _textLine.Value;
        }
        else
        {
            _printTextCoroutine = StartCoroutine(PrintText());
            CurrentLine++;
        }
    }

    public void GetScenario(AsyncOperationHandle<TextAsset> scenario)
    {
        if (scenario.Status == AsyncOperationStatus.Succeeded)
        {
            var scenarioXml = XDocument.Parse(scenario.Result.text).Element("root").Element("scenario");
            _textLines = scenarioXml.Elements("textLine").ToArray();
        }
    }

    /// <summary>
    /// Switching to the required text line
    /// </summary>
    /// <param name="lineNum">Required TextLine Number</param>
    /// <param name="isForce">If false, line changing occurs after the tap</param>
    public void ChangeCurrentLine(int lineNum, bool isForce)
    {
        CurrentLine = lineNum;

        if (!isForce) return;

        if (_printTextCoroutine != null)
        {
            StopCoroutine(_printTextCoroutine);
            _printTextCoroutine = null;
        }
        ShowNextTextLine();
    }

    private void Start()
    {
        _scenarioHandle = Addressables.LoadAssetAsync<TextAsset>("TestScenarioXml");
        _scenarioHandle.Completed += GetScenario;
        Lean.Touch.LeanTouch.OnFingerTap += ShowNextTextLine;
    }

    private void ProcessAttributes(XElement element)
    {
        if (element == null) return;

        foreach (var attribute in element.Attributes())
        {
            var type = Type.GetType("Commands");
            var method = type.GetMethod(Commands.s_Commands[attribute.Name.ToString()]);
            var parameters = attribute.Value.Split(",");
            method.Invoke(_commands, new object[] { parameters });
        }
    }

    private void ProcessAttributes(XElement element, string subElement)
    {
        if (element == null) return;

        foreach (var sub in element.Elements(subElement))
        {
            ProcessAttributes(sub);
        }
    }
}
