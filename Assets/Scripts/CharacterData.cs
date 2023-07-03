using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Character")]
public class CharacterData : ScriptableObject
{

    public string Name { get; set; }
    public string Clothes { get; set; }
    public string Emote { get; set; }
    public string Extra { get; set; } // extra description. Example: Blush
    public string Pose { get; set; }

    public CharacterData(string name, string clothes, string emote, string extra, string pose)
    {
        Name = name;
        Clothes = clothes;
        Emote = emote;
        Extra = extra;
        Pose = pose;
    }
}
