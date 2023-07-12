using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LTouch = Lean.Touch.LeanTouch;
using LFinger = Lean.Touch.LeanFinger;

/// <summary>
/// Swipe and tap management class
/// </summary>
public class InputSystem : MonoBehaviour
{
    public bool IsSwipedActionActived;
    public bool IsSkipping;
    public bool IsTTSEnabled;

    [SerializeField] private GameObject _ui;
    [SerializeField] private GameObject _history;
    [SerializeField] private TextAppear _textAppear;
    [SerializeField] private TTS _tts;

    private Coroutine _skipCoroutine;

    public void CheckDirection(LFinger finger)
    {
        if (IsSwipedActionActived)
        {
            ReturnToDefault();
        }

        var fingerDelta = Mathf.Abs(finger.SwipeScreenDelta.x) > Mathf.Abs(finger.SwipeScreenDelta.y)
                          ? (finger.SwipeScreenDelta.x > 0 ? Direction.Right : Direction.Left)
                          : (finger.SwipeScreenDelta.y > 0 ? Direction.Up : Direction.Down);

        SwipeTransport(fingerDelta);
    }

    public void SwipeTransport(Direction direction)
    {
        switch (direction)
        {
            case Direction.Right:
                StartSkip();
                break;
            case Direction.Left:
                ShowHistory();
                break;
            case Direction.Up:
                Application.Quit();
                break;
            case Direction.Down:
                HideUI();
                break;
            default:
                break;
        }
    }

    public void HideUI()
    {
        _ui?.SetActive(false);
        IsSwipedActionActived = true;
    }

    public void ReturnToDefault()
    {
        StopSkip();
        _ui?.SetActive(true);
        _history?.SetActive(false);
        IsSwipedActionActived = false;
    }

    public void ShowHistory()
    {
        _history?.SetActive(true);
        IsSwipedActionActived = true;
    }

    private IEnumerator Skip()
    {
        while(IsSkipping)
        {
            _textAppear.ShowNextTextLine();
            yield return new WaitForSeconds(0.1f);
        }
        _skipCoroutine = null;
    }

    private void StartSkip()
    {
        StopSkip();

        IsSkipping = true;
        IsSwipedActionActived = true;
        _skipCoroutine = StartCoroutine(Skip());
    }

    public void StopSkip()
    {
        IsSkipping = false;

        if (_skipCoroutine != null)
        {
            StopCoroutine(_skipCoroutine);
            _skipCoroutine = null;
        }
    }

    private void EnableTTS(LFinger finger)
    {
        IsTTSEnabled = !IsTTSEnabled;

        var startText = "Синтез речи ";
        startText += IsTTSEnabled ? "включен" : "выключен";

        _tts.Speak(startText);
    }

    private void Start()
    {
        LTouch.OnFingerSwipe += CheckDirection;
        LTouch.OnFingerOld += EnableTTS;
    }
}
