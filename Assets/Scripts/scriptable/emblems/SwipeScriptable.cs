using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardMatch;

[CreateAssetMenu(fileName = "MatchEmblem",menuName = "WizardMatch/MatchEmblem", order = 0)]


// scriptable object for holding basic data about the swipe icons
public class SwipeScriptable : ScriptableObject
{
    public Sprite icon;
    public Element swipeElement;
    public short color;
    /*
        0 = red
        1 = green
        2 = blue
        3 = white
        4 = purple
        5 = yellow
        6+= black / grey
    */
}
