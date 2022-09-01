using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sprite Data", menuName = "Scriptable Objects/Sprite Data")] 
public class SpriteData : ScriptableObject
{
    public Sprite[] forward, right, backward, left;

    public Color[] colours;
}
