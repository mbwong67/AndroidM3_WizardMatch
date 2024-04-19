using System.Collections.Generic;
using UnityEngine;
namespace WizardMatch
{
    
    public struct ParentTokenMatchData
    {
        public WizardToken parentToken;
        public WizardToken highestScoringToken;
        public List<WizardToken> affectedTokens;
        public MatchType matchType;
        public int count;
        
        public ParentTokenMatchData(WizardToken parentToken,List<WizardToken> list, MatchType matchType = MatchType.NO_MATCH )
        {
            this.parentToken = parentToken;
            highestScoringToken = this.parentToken;
            this.matchType = matchType;

            affectedTokens = list;
            count = 0;
        }
    }
    public enum TokenState
    {
        IDLE,
        MOVING,
        SWIPING,
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
    public enum Element
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
    public enum TokenUpgradeType
    {
        DEFAULT,
        BOMB,
        CROSS,
        TURBO
    }
    public enum GameState
    {
        READY,
        CHECK_SWIPE,
        MATCHING,
        CASCADE,
        WAIT_FOR_CASCADE,
        RETURN,
        FINAL_CHECK_BEFORE_ATTACK,
        ENEMY_TURN,
        WAIT_GENERAL,
        ENEMY_ATTACKING,
        FRIENDLY_ATTACKING,
        WIN,
        NONE
    };
    public enum CharacterType
    {
        PLAYER,
        ENEMY,
        BOSS
    }
    
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

        private static List<WizardToken> _likeTokens = new List<WizardToken>();
        public static List<WizardToken> likeTokens {get { return _likeTokens; } private set { _likeTokens = value;} }

        /// <summary>
        /// Find all like colors of a particular token.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="currentDepth"></param>
        /// <param name="lastTraveledDirection"></param>
        /// <returns></returns>
        public static ParentTokenMatchData FloodMatchTokens(WizardToken token)
        {
            // if this token has already been declared a match, return immediately.
            List<WizardToken> floodTokens = new List<WizardToken>();
            ParentTokenMatchData ret = new ParentTokenMatchData(token,floodTokens);

            FloodMatchTokensHelper(token,ret);
            ret.affectedTokens.AddRange(_likeTokens);
            ret.count = _likeTokens.Count;
            _likeTokens.Clear();
            return ret;


        }
        private static void FloodMatchTokensHelper(WizardToken token,ParentTokenMatchData list, SwipeDirection lastTraveledDirection = SwipeDirection.NONE)
        {
            if (token.visited)
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
                        _likeTokens.Add(token);
                        token.visited = true;

                        FloodMatchTokensHelper(token.northNeighbor,list,SwipeDirection.UP);
                        FloodMatchTokensHelper(token.northNeighbor,list,SwipeDirection.LEFT);
                        FloodMatchTokensHelper(token.northNeighbor,list,SwipeDirection.RIGHT);
                    }
                    return;
                case SwipeDirection.DOWN :
                    if (token.southNeighbor && token.southNeighbor.realColor == token.realColor)
                    {
                        path.Enqueue(SwipeDirection.DOWN);
                        _likeTokens.Add(token);
                        token.visited = true;
                        
                        FloodMatchTokensHelper(token.southNeighbor,list,SwipeDirection.DOWN);
                        FloodMatchTokensHelper(token.southNeighbor,list,SwipeDirection.LEFT);
                        FloodMatchTokensHelper(token.southNeighbor,list,SwipeDirection.RIGHT);
                    }
                    return;
                case SwipeDirection.LEFT :
                    if (token.westNeighbor && token.westNeighbor.realColor == token.realColor)
                    {
                        path.Enqueue(SwipeDirection.LEFT);
                        _likeTokens.Add(token);
                        token.visited = true;

                        FloodMatchTokensHelper(token.westNeighbor,list,SwipeDirection.UP);
                        FloodMatchTokensHelper(token.westNeighbor,list,SwipeDirection.LEFT);
                        FloodMatchTokensHelper(token.westNeighbor,list,SwipeDirection.DOWN);
                    }
                    return;
                case SwipeDirection.RIGHT :
                    if (token.eastNeighbor && token.eastNeighbor.realColor == token.realColor)
                    {
                        path.Enqueue(SwipeDirection.RIGHT);
                        _likeTokens.Add(token);
                        token.visited = true;
                        FloodMatchTokensHelper(token.eastNeighbor,list,SwipeDirection.UP);
                        FloodMatchTokensHelper(token.eastNeighbor,list,SwipeDirection.RIGHT);
                        FloodMatchTokensHelper(token.eastNeighbor,list,SwipeDirection.DOWN);
                    }
                    return;

                // No direction was made, so we are at our initial step. Check all directions.
                default :
                    FloodMatchTokensHelper(token,list,SwipeDirection.UP);
                    FloodMatchTokensHelper(token,list,SwipeDirection.DOWN);
                    FloodMatchTokensHelper(token,list,SwipeDirection.LEFT);
                    FloodMatchTokensHelper(token,list,SwipeDirection.RIGHT);
                    break;
            }
            return;

        }
        /// <summary>
        /// Loop through the parent data's affected tokens and determine the highest match type within.
        /// </summary>
        /// <param name="parentData"></param>
        /// <param name="highestMatchType"></param>
        public static void DetermineParentTokenMatchType(ref ParentTokenMatchData parentData, out MatchType highestMatchType)
        {
            foreach(WizardToken token in parentData.affectedTokens)
            {
                CheckTokenForMatches(token);
                if (token.matchType > parentData.matchType)
                {
                    parentData.highestScoringToken = token;
                    parentData.matchType = token.matchType;
                }
            }
            highestMatchType = parentData.matchType;
            if (parentData.highestScoringToken.matchType > MatchType.THREE_IN_A_ROW)
            {
                if (parentData.highestScoringToken.upgradeType == TokenUpgradeType.DEFAULT)
                    parentData.highestScoringToken.shouldUpgrade = true;
                switch (parentData.matchType)
                {
                    case MatchType.FOUR_IN_A_ROW :
                        parentData.highestScoringToken.upgradeType = TokenUpgradeType.BOMB;
                        break;
                    case MatchType.CROSS :
                        parentData.highestScoringToken.upgradeType = TokenUpgradeType.CROSS;
                        break;
                    case MatchType.FIVE_IN_A_ROW :
                        parentData.highestScoringToken.upgradeType = TokenUpgradeType.TURBO;
                        break;                                                
                }
                parentData.highestScoringToken.shouldUpgrade = true;
            }
        }
        public static MatchType GetMatchType(int xCount, int yCount)
        {
            MatchType ret;

            // takes highest priority. if we have a 5 in a row, don't consider any other possibilities
            if (xCount >= 4 || yCount >= 4)
                ret = MatchType.FIVE_IN_A_ROW;
            
            else if (xCount == 3 || yCount == 3)
            {
                ret = MatchType.FOUR_IN_A_ROW;
            }

            else if (xCount >= 2 && yCount >= 2)
                ret = MatchType.CROSS;
            // three in a row
            else if (xCount == 2 || yCount == 2 )
                ret =  MatchType.THREE_IN_A_ROW;
            // no match
            else
                ret = MatchType.NO_MATCH;
            
            return ret;
        }
        private static void CheckTokenForMatches(WizardToken token)
        {
            token.likeHorizontalNeighbors.Clear();
            token.likeVerticalNeighbors.Clear();
            
            int xCount = 
                token.CountNeighborsInCertainDirection(token,SwipeDirection.LEFT) + token.CountNeighborsInCertainDirection(token,SwipeDirection.RIGHT);
            int yCount = 
                token.CountNeighborsInCertainDirection(token,SwipeDirection.DOWN) + token.CountNeighborsInCertainDirection(token,SwipeDirection.UP);

            token.matchType = GetMatchType(xCount,yCount);
            // if we haven't gotten a match, return gamestate RETURN 
            if (token.matchType == MatchType.NO_MATCH)
                return;
            token.matched = true;
            if (xCount > yCount || yCount < 1)
            {
                token.likeVerticalNeighbors.Clear();
            }
            if (yCount > xCount || xCount < 1)
            {
                token.likeHorizontalNeighbors.Clear();
            }
            foreach(WizardToken neighborToken in token.likeHorizontalNeighbors)
            {
                neighborToken.matched = true;
            }
            foreach(WizardToken neighborToken in token.likeVerticalNeighbors)
            {
                neighborToken.matched = true;
            }
            if (token.matchType > MatchType.THREE_IN_A_ROW)
                token.shouldUpgrade = true;

        }
    }
}