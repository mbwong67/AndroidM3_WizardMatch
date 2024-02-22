using System.Collections.Generic;

namespace WizardMatch
{
    public enum SwipeDirection
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }
    public enum SwipeElement
    {
        FIRE,
        EARTH,
        WATER,
        AIR,
        OTHER,
        NONE
    };
    public enum MatchType
    {
        NONE,
        THREE_IN_A_ROW,
        FOUR_IN_A_ROW,
        CROSS,
        FIVE_IN_A_ROW
    };
}