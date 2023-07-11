using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parameters management class
/// </summary>
public class GameParameters : MonoBehaviour
{
    public static Dictionary<string, int> s_Parameters;
    public static List<Dictionary<string, int>> s_ParametersHistory;

    private void Start()
    {
        s_Parameters = new Dictionary<string, int>();
        s_ParametersHistory = new List<Dictionary<string, int>>();
    }

    public static void ChangeParameter(string parameter, int shift)
    {
        s_ParametersHistory.Add(new Dictionary<string, int>(s_Parameters));

        if (!s_Parameters.ContainsKey(parameter))
        {
            s_Parameters.Add(parameter, 0);
        }

        s_Parameters[parameter] += shift;
    }
}
