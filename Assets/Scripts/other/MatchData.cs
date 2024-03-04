using System.Collections.Generic;
using UnityEngine;
namespace WizardMatch
{
    public struct ParentTokenMatchData // <-- unknown if needed yet
    {
        List<WizardToken> affectedTokens;
        MatchType matchType;
    }
    public enum TokenState
    {
        IDLE,
        MOVING,
        FALLING,
        DESTROYING
    }
    public enum SwipeDirection
    {
        UP,
        LEFT,
        DOWN,
        RIGHT,
        NONE
    }
    public enum SwipeElement
    {
        FIRE,
        EARTH,
        WATER,
        AIR,
        DARK,
        LIGHT,
        OTHER,
        NONE
    };
    public enum MatchType
    {
        NO_MATCH,
        THREE_IN_A_ROW,
        FOUR_IN_A_ROW,
        CROSS,
        FIVE_IN_A_ROW
    };
    public enum GameState
    {
        READY,
        CHECK_SWIPE,
        MATCHING,
        WAIT,
        RETURN,
        NONE
    };
    /// <summary>
    /// Class for finding any matches on a board when called upon.
    /// </summary>
    public static class BoardSolver
    {
        /// <summary>
        /// [0] == North ||
        /// [1] == West ||
        /// [2] == South ||
        /// [3] == East
        /// </summary>
        public static int[] NumOfDirectionsTraveled = {0,0,0,0};
        public static Queue<SwipeDirection> path = new Queue<SwipeDirection>();

        private static List<WizardToken> _matchingTokens = new List<WizardToken>();
        public static List<WizardToken> matchingTokens {get { return _matchingTokens; } private set { _matchingTokens = value;} }

        /// <summary>
        /// Find the shape and tokens of a particular match (if any is present). 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="currentDepth"></param>
        /// <param name="lastTraveledDirection"></param>
        /// <returns></returns>
        public static void FindMatchesStartingAtToken(WizardToken token, int currentDepth = 0, SwipeDirection lastTraveledDirection = SwipeDirection.NONE)
        {
            // if this token has already been declared a match, return immediately.
            if (token.matched)
                return;
            switch(lastTraveledDirection) 
            {
                // comments for up direction cuz then it'll explain the rest.
                case SwipeDirection.UP :
                    // if the token's northern neighbor exists and is the same color;
                    //  add 1 to the number of times we traveled north
                    //  
                    if (token.northNeighbor && token.northNeighbor.realColor == token.realColor)
                    {
                        path.Enqueue(SwipeDirection.UP);
                        NumOfDirectionsTraveled[0]++;
                        token.matched = true;
                        _matchingTokens.Add(token);
                        Debug.Log("going up");
                        FindMatchesStartingAtToken(token.northNeighbor,currentDepth++,SwipeDirection.UP);
                        FindMatchesStartingAtToken(token.northNeighbor,currentDepth++,SwipeDirection.LEFT);
                        FindMatchesStartingAtToken(token.northNeighbor,currentDepth++,SwipeDirection.RIGHT);
                    }
                    return;
                case SwipeDirection.DOWN :
                    if (token.southNeighbor && token.southNeighbor.realColor == token.realColor)
                    {
                        path.Enqueue(SwipeDirection.DOWN);
                        NumOfDirectionsTraveled[2]++;
                        token.matched = true;
                        _matchingTokens.Add(token);
                        Debug.Log("going down");
                        
                        FindMatchesStartingAtToken(token.southNeighbor,currentDepth++,SwipeDirection.DOWN);
                        FindMatchesStartingAtToken(token.southNeighbor,currentDepth++,SwipeDirection.LEFT);
                        FindMatchesStartingAtToken(token.southNeighbor,currentDepth++,SwipeDirection.RIGHT);
                    }
                    return;
                case SwipeDirection.LEFT :
                    if (token.westNeighbor && token.westNeighbor.realColor == token.realColor)
                    {
                        path.Enqueue(SwipeDirection.LEFT);
                        NumOfDirectionsTraveled[1]++;
                        token.matched = true;
                        _matchingTokens.Add(token);
                        Debug.Log("going left");
                        FindMatchesStartingAtToken(token.westNeighbor,currentDepth++,SwipeDirection.UP);
                        FindMatchesStartingAtToken(token.westNeighbor,currentDepth++,SwipeDirection.LEFT);
                        FindMatchesStartingAtToken(token.westNeighbor,currentDepth++,SwipeDirection.DOWN);
                    }
                    return;
                case SwipeDirection.RIGHT :
                    if (token.eastNeighbor && token.eastNeighbor.realColor == token.realColor)
                    {
                        path.Enqueue(SwipeDirection.RIGHT);
                        NumOfDirectionsTraveled[3]++;
                        token.matched = true;
                        _matchingTokens.Add(token);
                        Debug.Log("going right");
                        FindMatchesStartingAtToken(token.eastNeighbor,currentDepth++,SwipeDirection.UP);
                        FindMatchesStartingAtToken(token.eastNeighbor,currentDepth++,SwipeDirection.RIGHT);
                        FindMatchesStartingAtToken(token.eastNeighbor,currentDepth++,SwipeDirection.DOWN);
                    }
                    return;

                // No direction was made, so we are at our initial step. Check all directions.
                default :
                    FindMatchesStartingAtToken(token,currentDepth,SwipeDirection.UP);
                    FindMatchesStartingAtToken(token,currentDepth,SwipeDirection.DOWN);
                    FindMatchesStartingAtToken(token,currentDepth,SwipeDirection.LEFT);
                    FindMatchesStartingAtToken(token,currentDepth,SwipeDirection.RIGHT);
                    break;
            }
            _matchingTokens.Clear();
            return;
        }
        private static MatchType DetermineMatchType()
        {
            return MatchType.NO_MATCH;
        }
    }
}