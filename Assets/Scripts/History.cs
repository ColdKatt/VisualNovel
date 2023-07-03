using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class History : MonoBehaviour
{
    public static AsyncOperationHandle<GameObject> HistoryElementTemplate;
    [SerializeField] private Transform _historyContent;
    public static Transform HistoryContent;
    public static List<GameObject> Elements;
    public static List<int> TextLines;

    void Start()
    {
        Elements = new List<GameObject>();
        TextLines = new List<int>();
        HistoryElementTemplate = Addressables.LoadAssetAsync<GameObject>("HistoryElement");
        HistoryContent = _historyContent;
    }

    public static void CreateElement(string description, int lineNum)
    {
        if (HistoryElementTemplate.Status == AsyncOperationStatus.Succeeded)
        {
            var element = Instantiate(HistoryElementTemplate.Result, HistoryContent);
            var rect = element.GetComponent<RectTransform>();
            var button = element.GetComponent<Button>();
            var text = element.GetComponent<TMP_Text>();
            rect.localPosition = new Vector2(rect.localPosition.x, rect.localPosition.y - (rect.rect.height * (HistoryContent.childCount - 1)));
            text.text = description;
            Elements.Add(element);
            TextLines.Add(lineNum);
            button.onClick.AddListener(() => Restore(Elements.IndexOf(element)));
            Debug.Log($"E:{Elements.IndexOf(element)}");
        }
    }

    //Restore choice. Required Elements & TextLines & ParametersHistory with same indexes
    public static void Restore(int index)
    {
        var restoreLine = TextLines[index] - 1;
        GameParameters.s_Parameters = GameParameters.s_ParametersHistory[index];
        for (var i = index; i < Elements.Count;)
        {
            var element = Elements[index];
            Elements.RemoveAt(i);
            TextLines.RemoveAt(i);
            GameParameters.s_ParametersHistory.RemoveAt(i);
            Destroy(element);
        }
        TextAppear.ChangeCurrentLine(restoreLine);
    }
}
