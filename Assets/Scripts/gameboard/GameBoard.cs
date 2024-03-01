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
        [SerializeField] public List<SwipeScriptable> tokenTypes = new List<SwipeScriptable>();
        [SerializeField] public List<WizardToken> matchingTokens = new List<WizardToken>();
        [SerializeField] public List<WizardToken> likeVerticalTokens = new List<WizardToken>();
        [SerializeField] public List<WizardToken> likeHorizontalTokens = new List<WizardToken>();
        [SerializeField] public WizardToken[,] playFieldTokens = new WizardToken[8,8];
        [SerializeField] public Vector2 anchorPosition;

        //[SerializeField] private GameManager _manager;
        [SerializeField] private WizardToken _tokenPrefab;
        [SerializeField][Range(0.1f,10f)] private float _horizontalSpacing = 0.0f;
        [SerializeField][Range(0.1f,10f)] private float _verticalSpacing = 0.0f;

        void Awake()
        {
            InitializeBoard();
        }
        void Update(){}
        void InitializeBoard()
        {
            if (!_tokenPrefab)
            {
                Debug.Log("ERROR : " + gameObject.name + " : _tokenPrefab not set! Aborting...");
                Destroy(gameObject);
            }
            for (int col = 0; col < playFieldTokens.GetLength(0); col++)
            {
                for (int row = 0; row < playFieldTokens.GetLength(1); row++)
                {
                    Vector2 offset = anchorPosition + new Vector2(col * _horizontalSpacing, -row * _verticalSpacing);
                    WizardToken curTok = Instantiate(_tokenPrefab,offset,Quaternion.identity,gameObject.transform);
                    curTok.swipeData = tokenTypes[(int) Mathf.Floor(Random.Range(0,tokenTypes.Count))];
                    curTok.InitializeTokenAtStart(new Vector2Int(col,row), GetComponent<GameBoard>(),_horizontalSpacing,_verticalSpacing);
                    playFieldTokens[col,row] = curTok;
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

        #region Matching Logic
        public void CheckTokenForMatches(WizardToken token)
        {

            int xCount = 
                token.CountNeighborsInCertainDirection(token,SwipeDirection.LEFT) + token.CountNeighborsInCertainDirection(token,SwipeDirection.RIGHT);
            int yCount = 
                token.CountNeighborsInCertainDirection(token,SwipeDirection.DOWN) + token.CountNeighborsInCertainDirection(token,SwipeDirection.UP);

            // test for each match type
            // five in a row
            if (xCount >= 4 || yCount >= 4)
                token.matchType = MatchType.FIVE_IN_A_ROW;
            
            // this one is broken. will have to fix.
            // four in a row or cross. cross takes priority.
            else if (xCount == 3 || yCount == 3)
            {
                if (xCount >= 2 && yCount >= 2)
                    token.matchType = MatchType.CROSS;
                else
                    token.matchType = MatchType.FOUR_IN_A_ROW;
            }
            // three in a row
            else if (xCount == 2 || yCount == 2)
                token.matchType = MatchType.THREE_IN_A_ROW;
            // no match
            else
                token.matchType = MatchType.NO_MATCH;

            // if we haven't gotten a match, return gamestate RETURN 
            if (token.matchType == MatchType.NO_MATCH)
                return;

            token.matched = true;

            if (xCount > yCount)
                likeVerticalTokens.Clear();
            else if (yCount > xCount)
                likeHorizontalTokens.Clear();

            foreach(WizardToken neighborToken in likeHorizontalTokens)
            {
                neighborToken.matched = true;
                matchingTokens.Add(neighborToken);
            }
            foreach(WizardToken neighborToken in likeVerticalTokens)
            {
                neighborToken.matched = true;
                matchingTokens.Add(neighborToken);
            }

            matchingTokens.Add(token);

        }

        #endregion

        #region Setup Methods
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