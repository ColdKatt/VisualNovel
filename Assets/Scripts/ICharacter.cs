using System.Collections.Generic;
using UnityEngine;

interface ICharacter
{
    public string Name { get; set; }
    public List<Sprite> Sprites { get; set; }

    public void ChangeSprite();
    public void Move();
    public void Spawn();
}
