using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LTouch = Lean.Touch.LeanTouch;
using LFinger = Lean.Touch.LeanFinger;

public class InputSystem : MonoBehaviour
{
    public GameObject UI;
    public GameObject History;

    public bool IsSwipedActionActived;

    // Start is called before the first frame update
    void Start()
    {
        LTouch.OnFingerSwipe += CheckDirection;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CheckDirection(LFinger finger)
    {
        if (IsSwipedActionActived)
        {
            ReturnToDefault();
        }
        Debug.Log(finger.SwipeScreenDelta);
        var fingerDelta = Mathf.Abs(finger.SwipeScreenDelta.x) > Mathf.Abs(finger.SwipeScreenDelta.y)
                          ? (finger.SwipeScreenDelta.x > 0 ? "Right" : "Left")
                          : (finger.SwipeScreenDelta.y > 0 ? "Up" : "Down");
        Debug.Log(fingerDelta);
        SwipeTransport(fingerDelta);
    }

    public void SwipeTransport(string direction)
    {
        switch (direction)
        {
            case "Right":
                //skip
                break;
            case "Left":
                ShowHistory();
                break;
            case "Up":
                Application.Quit();
                break;
            case "Down":
                HideUI(direction);
                break;
            default:
                break;
        }
    }

    public void HideUI(string direction)
    {
        if (UI != null)
        {
            UI.SetActive(false);
        }
        Debug.Log("UI Hided");
        IsSwipedActionActived = true;
    }

    public void ReturnToDefault()
    {
        if (IsSwipedActionActived)
        {
            UI.SetActive(true);
            History.SetActive(false);
            IsSwipedActionActived = false;
            return;
        }
    }

    public void ShowHistory()
    {
        if (History != null)
        {
            History.SetActive(true);
            IsSwipedActionActived = true;
        }
    }
}
