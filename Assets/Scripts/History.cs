using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// Chooses history management class
/// </summary>
public class History : MonoBehaviour
{
    [SerializeField] private Commands _commands;
    [SerializeField] private Transform _historyContent;

    private static AsyncOperationHandle<GameObject> s_historyElementTemplateHandle;
    private static Transform s_historyContent;
    private static List<GameObject> s_elements;
    private static List<int> s_textLines;
    private static Commands s_commands;
    private static List<SceneData> s_sceneDataHistory;
    private static List<Dictionary<string, Character>> s_charactersHistory;
    private static List<Dictionary<string, float>> s_characterPositionsHistory;

    /// <summary>
    /// Creates a history element
    /// </summary>
    /// <param name="description">TMP_Text.text for element</param>
    /// <param name="lineNum"></param>
    public static void CreateElement(string description, int lineNum)
    {
        if (s_historyElementTemplateHandle.Status == AsyncOperationStatus.Succeeded)
        {
            var element = Instantiate(s_historyElementTemplateHandle.Result, s_historyContent);
            var rect = element.GetComponent<RectTransform>();
            var button = element.GetComponent<Button>();
            var text = element.GetComponent<TMP_Text>();

            rect.localPosition = new Vector2(rect.localPosition.x, rect.localPosition.y - (rect.rect.height * (s_historyContent.childCount - 1)));
            text.text = description;

            s_elements.Add(element);
            s_textLines.Add(lineNum);
            s_sceneDataHistory.Add(s_commands.SceneData);
            s_charactersHistory.Add(new Dictionary<string, Character>(Commands.s_Characters));

            var names = Commands.s_Characters.Keys.ToArray();
            var tempPositions = new Dictionary<string, float>();

            foreach (var name in names)
            {
                tempPositions.Add(name, Commands.s_Characters[name].GetPosition().x);
            }

            s_characterPositionsHistory.Add(new Dictionary<string, float>(tempPositions));

            button.onClick.AddListener(() => Restore(s_elements.IndexOf(element)));
        }
    }

    /// <summary>
    /// Restores choice event.
    /// </summary>
    /// <param name="index"></param>
    public static void Restore(int index)
    {
        s_commands.CancelChoose();

        var restoreLine = s_textLines[index] - 1;
        var sceneData = s_sceneDataHistory[index];
        var tempCharacters = new Dictionary<string, Character>(s_charactersHistory[index]);
        var tempPositions = new Dictionary<string, float>(s_characterPositionsHistory[index]);
        GameParameters.s_Parameters = new Dictionary<string, int>(GameParameters.s_ParametersHistory[index]);

        for (var i = index; i < s_elements.Count;)
        {
            var element = s_elements[index];

            ClearLists(i);

            Destroy(element);
        }

        var names = Commands.s_Characters.Keys.ToArray();

        foreach (var name in names)
        {
            s_commands.DeleteSprite(new string[] { name });
        }

        foreach (var name in tempCharacters.Keys)
        {
            s_commands.CreateCharacter(tempCharacters[name].GetCharacterData());
            Commands.s_Characters[name].SetPosition(Vector3.right * tempPositions[name]);
        }

        s_commands.SetBack(new string[] { sceneData.CurrentBack });
        s_commands.SetMusic(new string[] { sceneData.CurrentMusic });
        s_commands.TextAppear.ChangeCurrentLine(restoreLine, true);
    }

    private static void ClearLists(int i)
    {
        s_elements.RemoveAt(i);
        s_textLines.RemoveAt(i);
        s_sceneDataHistory.RemoveAt(i);
        s_charactersHistory.RemoveAt(i);
        s_characterPositionsHistory.RemoveAt(i);
        GameParameters.s_ParametersHistory.RemoveAt(i);
    }

    private void Start()
    {
        s_elements = new List<GameObject>();
        s_textLines = new List<int>();
        s_sceneDataHistory = new List<SceneData>();
        s_charactersHistory = new List<Dictionary<string, Character>>();
        s_characterPositionsHistory = new List<Dictionary<string, float>>();
        s_historyElementTemplateHandle = Addressables.LoadAssetAsync<GameObject>("HistoryElement");
        s_historyContent = _historyContent;
        s_commands = _commands;
    }
}
