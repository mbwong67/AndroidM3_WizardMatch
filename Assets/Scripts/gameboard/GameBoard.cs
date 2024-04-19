using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace WizardMatch
{
    // class for housing and positioning swipeable tokens and items. 
    // calculates positions, matches, and relays state information with the game manager. 
    public class GameBoard : MonoBehaviour
    {
        public delegate void RecalculateBoardPosition();
        public static event RecalculateBoardPosition UpdateBoardPosition;

        /// <summary>
        /// If we get any matches that are above just a regular match, add it here.
        /// </summary>
        [SerializeField] public int matchCombo = 0;
        [SerializeField] public int specialTokenModifier = 1;
        [SerializeField] public Vector2 anchorPosition;
        [SerializeField] public List<SwipeScriptable> tokenTypes = new List<SwipeScriptable>();
        [SerializeField] public List<WizardToken> matchedTokens = new List<WizardToken>();
        // tokens in this list are there specifically to check for matches after a new board has been filled.
        [SerializeField] public List<WizardToken> tokensToCheckAfterMatch = new List<WizardToken>();
        [SerializeField] public WizardToken[,] playFieldTokens = new WizardToken[8,8];
        [SerializeField] public bool boardIsStill = false;

        [SerializeField] private WizardToken _tokenPrefab;
        [SerializeField][Range(0.1f,10f)] private float _horizontalSpacing = 0.0f;
        [SerializeField][Range(0.1f,10f)] private float _verticalSpacing = 0.0f;


        void Update()
        {
            for (int i = 0; i < playFieldTokens.GetLength(0); i++)
            {
                for (int j = 0; j < playFieldTokens.GetLength(1); j++)
                {
                    if (playFieldTokens[i,j] && playFieldTokens[i,j].tokenState == TokenState.IDLE)
                    {
                        boardIsStill = true;
                    }
                    else
                    {
                        boardIsStill = false;
                        return;
                    }
                }
            }
        }
        void MonitorTokens()
        {
            if (boardIsStill)
            {
                foreach(WizardToken token in playFieldTokens)
                {
                    token.ForceMoveInstant(token.boardPosition);
                }
            }
        }
        public bool CheckWholeBoardForMatches()
        {
            foreach(WizardToken token in playFieldTokens)
            {
                CheckTokenForMatches(token);
                if (token.matchType != MatchType.NO_MATCH)
                    matchedTokens.Add(token);
            }
            if (matchedTokens.Count > 0)
                return true;
            return false;
        }

        #region Matching Logic
        public void CheckTokenForMatches(WizardToken token)
        {
            token.likeHorizontalNeighbors.Clear();
            token.likeVerticalNeighbors.Clear();

            int xCount = 
                token.CountNeighborsInCertainDirection(token,SwipeDirection.LEFT) + token.CountNeighborsInCertainDirection(token,SwipeDirection.RIGHT);
            int yCount = 
                token.CountNeighborsInCertainDirection(token,SwipeDirection.DOWN) + token.CountNeighborsInCertainDirection(token,SwipeDirection.UP);

            token.matchType = BoardSolver.GetMatchType(xCount,yCount);
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
                matchedTokens.Add(neighborToken);
            }
            foreach(WizardToken neighborToken in token.likeVerticalNeighbors)
            {
                neighborToken.matched = true;
                matchedTokens.Add(neighborToken);
            }
            if (token.matchType > MatchType.THREE_IN_A_ROW)
            {
                specialTokenModifier++;
                if (token.upgradeType == TokenUpgradeType.DEFAULT)
                    token.shouldUpgrade = true;
                switch (token.matchType)
                {
                    case MatchType.FOUR_IN_A_ROW :
                        token.upgradeType = TokenUpgradeType.BOMB;
                        break;
                    case MatchType.CROSS :
                        specialTokenModifier += 1;
                        token.upgradeType = TokenUpgradeType.CROSS;
                        break;
                    case MatchType.FIVE_IN_A_ROW :
                        specialTokenModifier += 2;
                        token.upgradeType = TokenUpgradeType.TURBO;
                        break;                                                
                }
            }
            matchedTokens.Add(token);

        }

        public void BreakAndScore()
        {
            List<WizardToken> specialTokens = new List<WizardToken>();
            matchCombo++;
            
            foreach(WizardToken token in matchedTokens)
            {
                if (token.upgradeType > TokenUpgradeType.DEFAULT && !token.shouldUpgrade)
                {
                    specialTokens.AddRange(AddTokensBasedOnUpgradeType(token));
                }
            }
            matchedTokens.AddRange(specialTokens);
            if (matchedTokens.Count >= 5 && matchedTokens.Count < 10)
            {
                specialTokenModifier++;
            }
            else if (matchedTokens.Count >= 10 && matchedTokens.Count < 15)
            {
                specialTokenModifier += 3;
            }
            else if (matchedTokens.Count >= 15)
            {
                specialTokenModifier += 5;
            }

            tokensToCheckAfterMatch.Clear();
            
            foreach(WizardToken token in matchedTokens)
            {
                // if it isn't just a normal token, don't break it. 
                if (!token.shouldUpgrade)
                {
                    token.tokenState = TokenState.DESTROYING;
                    token.PlayAnimation("Destroyed");
                    playFieldTokens[token.boardPosition.x,token.boardPosition.y] = null;
                }
            }
            matchedTokens.Clear();
        }

        /// <summary>
        /// Big gross disgusting ugly mess.
        /// Grab tokens appropriate to the target token's upgrade type. If any token is 
        /// already upgraded, add appropriate tokens based on that as well.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        List<WizardToken> AddTokensBasedOnUpgradeType(WizardToken token)
        {
            TokenUpgradeType type = token.upgradeType;

            if (type == TokenUpgradeType.DEFAULT)
                return null;

            List<WizardToken> retList = new List<WizardToken>();

            switch (type)
            {
                case TokenUpgradeType.BOMB :
                    var list = token.GrabTokenNeighbors();
                    foreach(WizardToken neighbor in list)
                    {
                        if (!matchedTokens.Contains(neighbor))
                        {
                            if (neighbor.upgradeType > TokenUpgradeType.DEFAULT)
                                retList.AddRange(AddTokensBasedOnUpgradeType(neighbor));
                            retList.Add(neighbor);
                        }
                        
                    }
                    // grab top left
                    if (token.boardPosition.y > 0 && token.boardPosition.x > 0)
                    {
                        var t = playFieldTokens[token.boardPosition.x - 1, token.boardPosition.y - 1];
                        if (!matchedTokens.Contains(t))
                        {
                            if (t.upgradeType > TokenUpgradeType.DEFAULT)
                                retList.AddRange(AddTokensBasedOnUpgradeType(t));
                            retList.Add(t);
                        }
                    }
                    // grab top right
                    if (token.boardPosition.y > 0 && token.boardPosition.x < playFieldTokens.GetLength(0) - 1)
                    {
                        var t = playFieldTokens[token.boardPosition.x + 1, token.boardPosition.y - 1];
                        if (!matchedTokens.Contains(t))
                        {
                            if (t.upgradeType > TokenUpgradeType.DEFAULT)
                                retList.AddRange(AddTokensBasedOnUpgradeType(t));
                            retList.Add(t);
                        }

                    }
                    // grab bottom left
                    if (token.boardPosition.y < playFieldTokens.GetLength(1) - 1 && token.boardPosition.x > 0)
                    {
                        var t = playFieldTokens[token.boardPosition.x - 1, token.boardPosition.y + 1];
                        if (!matchedTokens.Contains(t))
                        {
                            if (t.upgradeType > TokenUpgradeType.DEFAULT)
                                retList.AddRange(AddTokensBasedOnUpgradeType(t));
                            retList.Add(t);
                        }
                    }
                    // grab bottom right
                    if (token.boardPosition.y < playFieldTokens.GetLength(1) - 1 && token.boardPosition.x < playFieldTokens.GetLength(0) - 1 )
                    {
                        var t = playFieldTokens[token.boardPosition.x + 1, token.boardPosition.y + 1];
                        if (!matchedTokens.Contains(t))
                        {
                            if (t.upgradeType > TokenUpgradeType.DEFAULT)
                                retList.AddRange(AddTokensBasedOnUpgradeType(t));
                            retList.Add(t);
                        }
                    }
                    break;
                case TokenUpgradeType.CROSS :
                    for (int x = 0; x < playFieldTokens.GetLength(0); x++)
                    {
                        var t = playFieldTokens[x,token.boardPosition.y];
                        if (t != token && !matchedTokens.Contains(t))
                        {
                            if (t.upgradeType > TokenUpgradeType.DEFAULT)
                                retList.AddRange(AddTokensBasedOnUpgradeType(t));
                            retList.Add(t);
                        }
                    }
                    for (int y = 0; y < playFieldTokens.GetLength(1); y++)
                    {
                        var t = playFieldTokens[token.boardPosition.x,y];
                        if (t != token && !matchedTokens.Contains(t))
                        {
                            if (t.upgradeType > TokenUpgradeType.DEFAULT)
                                retList.AddRange(AddTokensBasedOnUpgradeType(t));
                            retList.Add(t);
                        }
                    }
                    break;
                case TokenUpgradeType.TURBO :
                    foreach(WizardToken t in playFieldTokens)
                    {
                        if (t != token && !matchedTokens.Contains(t) && t.realColor == token.realColor)
                        {
                            if (t.upgradeType > TokenUpgradeType.DEFAULT)
                                retList.AddRange(AddTokensBasedOnUpgradeType(t));
                            retList.Add(t);
                        }
                    }
                    break;
            }
            return retList;
        }
        #endregion

        #region Repopulating Logic
        public void RepopulateBoard()
        {
            List<Vector2Int> lowestPositions = FindLowestEmptyPositions();
    
            foreach(Vector2Int lowPosition in lowestPositions)
            {
                SnapToLowest(lowPosition,lowPosition);
            }
            var emptyPositions = FindEmptyPositions();
            foreach(Vector2Int emptyPosition in emptyPositions)
            {
                CreateTokenAtPoint(emptyPosition.x,emptyPosition.y);
                tokensToCheckAfterMatch.Add(playFieldTokens[emptyPosition.x,emptyPosition.y]);
            }
            SnapAllTokensToAppropriatePositions();
        }
        public void Cascade()
        {
            ParentTokenMatchData[] parentTokens = new ParentTokenMatchData[64];
            boardIsStill = false;
            
            // flood fill each token and return their like tokens and parent token into parentData. add it to a list
            // of parent tokens to check for the highest match among them. 
            int count = 0;
            foreach(WizardToken token in tokensToCheckAfterMatch)
            {
                ParentTokenMatchData parentData = BoardSolver.FloodMatchTokens(token);
                parentTokens[count] = parentData;
                count++;
            }

            // then, with the list of parent tokens in hand, loop through each of their affected tokens and determine
            // the highest match amongst them. 
            for (int i = 0; i < count; i++)
            {
                BoardSolver.DetermineParentTokenMatchType(ref parentTokens[i], out MatchType highestMatchType);
                if (highestMatchType != MatchType.NO_MATCH)
                {
                    matchedTokens.AddRange(parentTokens[i].highestScoringToken.likeHorizontalNeighbors);
                    matchedTokens.AddRange(parentTokens[i].highestScoringToken.likeVerticalNeighbors);
                    matchedTokens.Add(parentTokens[i].highestScoringToken);
                }    
            }
            if (matchedTokens.Count > 0)
            {
                BreakAndScore();
            }
            for (int x = 0; x < playFieldTokens.GetLength(0); x++)
            {
                for (int y = 0; y < playFieldTokens.GetLength(1); y++)
                {
                    if (playFieldTokens[x,y])
                        playFieldTokens[x,y].visited = false;
                }
            }
            tokensToCheckAfterMatch.Clear();
        }

        public void ResetTokens()
        {
            UpdateBoardPosition();
            PopulateNeighborTokens();
            foreach(WizardToken token in playFieldTokens)
            {
                token.matched = false;
                token.shouldUpgrade = false;
            }

        }
        void SnapToLowest(Vector2Int snapPos, Vector2Int curPosition)
        {
            if (curPosition.y < 0)

                return;
            int posX = curPosition.x;
            int posY = curPosition.y;
            // if there is a token at this position, add it to the list
            // of tokens to check after the match. these will need to 
            // be checked for matches. 
            if (playFieldTokens[posX,posY])
            {
                WizardToken affectedToken = playFieldTokens[posX,posY];
                affectedToken.MoveToEmptyBoardPosition(snapPos);
                tokensToCheckAfterMatch.Add(affectedToken);
                Vector2Int newSnapPosition = new Vector2Int(snapPos.x,snapPos.y - 1);
                SnapToLowest(newSnapPosition,newSnapPosition);
            }
            else
                SnapToLowest(snapPos,new Vector2Int(posX,posY - 1));
        }

        List<Vector2Int> FindEmptyPositions()
        {
            List<Vector2Int> emptyPositions = new List<Vector2Int>();
            for (int col = 0; col < playFieldTokens.GetLength(0); col++)
            {
                for (int row = 0; row < playFieldTokens.GetLength(1); row++)
                {
                    if (playFieldTokens[col,row] == null)
                    {
                        emptyPositions.Add(new Vector2Int(col,row));
                    }
                }
            }
            return emptyPositions;
        }
        List<Vector2Int> FindLowestEmptyPositions()
        {
            List<Vector2Int> emptyPositions = FindEmptyPositions();
            List<Vector2Int> lowestPositions = new List<Vector2Int>();

            for (int i = 0; i < playFieldTokens.GetLength(0); i++)
            {
                var tempList = emptyPositions.FindAll(s => s.x == i); // find all positions whose x value matches the index
                if (tempList.Count > 0)
                {
                    Vector2Int add = PruneLowestPositions(tempList);
                    lowestPositions.Add(PruneLowestPositions(tempList));
                }
            }
            
            return lowestPositions;
        }

        Vector2Int PruneLowestPositions(List<Vector2Int> list)
        {
            Vector2Int ret = new Vector2Int(-1,-1);
            int highestY = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].y > highestY)
                {
                    ret = list[i];
                }
            }
            return ret;
        }
        public void SnapAllTokensToAppropriatePositions()
        {
            foreach(WizardToken token in playFieldTokens)
                token.ForceMove(token.boardPosition);
        }
        #endregion

        #region Setup Methods
        void CreateTokenAtPoint(int col, int row)
        {
            Vector2 offset = anchorPosition + new Vector2(col * _horizontalSpacing, -row * _verticalSpacing) + (Vector2) transform.position;
            WizardToken curTok = Instantiate(_tokenPrefab,offset,Quaternion.identity,gameObject.transform);
            curTok.swipeData = tokenTypes[(int) Mathf.Floor(Random.Range(0,tokenTypes.Count))];
            curTok.InitializeTokenAtStart(new Vector2Int(col,row), GetComponent<GameBoard>(),_horizontalSpacing,_verticalSpacing);
            playFieldTokens[col,row] = curTok;
        }
        public void InitializeBoard()
        {
            if (!_tokenPrefab)
            {
                Debug.LogError("ERROR : " + gameObject.name + " : _tokenPrefab not set! Aborting...");
                Destroy(gameObject);
            }
            _horizontalSpacing *= transform.localScale.x;
            _verticalSpacing *= transform.localScale.y;
            anchorPosition *= transform.localScale;
            for (int col = 0; col < playFieldTokens.GetLength(0); col++)
            {
                for (int row = 0; row < playFieldTokens.GetLength(1); row++)
                {
                    CreateTokenAtPoint(col,row);
                }
            }
            PopulateNeighborTokens();
            for (int x = 0 ; x < playFieldTokens.GetLength(0); x++)
                for (int y = 0 ; y < playFieldTokens.GetLength(1); y++)
                {
                    SetupCheckForUpwardMatches(playFieldTokens[x,y]);
                    SetupCheckForLeftwardMatches(playFieldTokens[x,y]);
                }
        }
        void PopulateNeighborTokens()
        {
            for (int x = 0; x < playFieldTokens.GetLength(0); x++)
            {
                for (int y = 0; y < playFieldTokens.GetLength(0); y++)
                {
                    if (x > 0)
                        playFieldTokens[x,y].westNeighbor = playFieldTokens[x-1,y];
                    if (y > 0)
                        playFieldTokens[x,y].northNeighbor = playFieldTokens[x,y-1];
                    if (x < playFieldTokens.GetLength(0) - 1)
                        playFieldTokens[x,y].eastNeighbor = playFieldTokens[x + 1, y];
                    if (y < playFieldTokens.GetLength(0) - 1) 
                        playFieldTokens[x,y].southNeighbor = playFieldTokens[x, y + 1];
                }
            }
        }
        void SetupCheckForUpwardMatches( WizardToken token)
        {
            if (token.boardPosition.y <= 1)
                return;
            WizardToken upNeighbor = token.northNeighbor;
            WizardToken upmostNeighbor = token.northNeighbor.northNeighbor;

            // first check. make sure that we aren't forming a line with our colors.
            bool firstMatch = upNeighbor.realColor == token.realColor;
            bool secondMatch = upNeighbor.realColor == upmostNeighbor.realColor;

            if (firstMatch && secondMatch)
            {
                int newColor = GenerateRandomColorDifferentFromNeighbors(token,upNeighbor,token.westNeighbor);
                token.SetColor(newColor);
                SetupCheckForUpwardMatches(upNeighbor);
            }
        }
        void SetupCheckForLeftwardMatches(WizardToken token)
        {
            if (token.boardPosition.x <= 1)
                return;
            WizardToken leftNeighbor = token.westNeighbor;
            WizardToken leftmostNeighbor = token.westNeighbor.westNeighbor ? token.westNeighbor.westNeighbor : null;

            // first check. make sure that we aren't forming a line with our colors.
            bool firstMatch = leftNeighbor.realColor == token.realColor;
            bool secondMatch = leftNeighbor.realColor == leftmostNeighbor.realColor;

            if (firstMatch && secondMatch)
            {
                // if our two colors do match, we want to generate a new color. make sure that this color doesn't match 
                // the color of our north neighbor on the off chance that our token accidentally makes a match in the
                // upwards direction. 
                int newColor = GenerateRandomColorDifferentFromNeighbors(token,leftNeighbor,token.northNeighbor);
                token.SetColor(newColor);
                SetupCheckForLeftwardMatches(leftNeighbor);
            }
        }
        int GenerateRandomColorDifferentFromNeighbors(WizardToken A, WizardToken B, WizardToken C = null)
        {
            int colorA = A.realColor;
            int colorB = B.realColor;
            int colorC = C? C.realColor : -1;

            while (true)
            {
                int newColor = (int) Mathf.Floor(Random.Range(0,tokenTypes.Count));
                if (newColor != colorA && newColor != colorB && (colorC != -1 ? newColor != colorC : true))
                    return newColor;
            }
        }
        #endregion
    }
}