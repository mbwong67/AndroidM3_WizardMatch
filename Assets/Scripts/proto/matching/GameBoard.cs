using System.Collections.Generic;
using proto;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using WizardMatch;

public class GameBoard : MonoBehaviour
{
    public List<SwipeScriptable> swipeScriptables = new List<SwipeScriptable>();
    public SwipeToken[,] playFieldTokens = new SwipeToken[8,8]; 
    public float horizontalSpacing = 0.5f;
    public float verticalSpacing = 0.5f;
    public Vector2 startPosition = new Vector3();


    [SerializeField] [Range(0.1f,200.0f)] private float _minimumRequiredScreenSwipeDelta = 10.0f;
    [SerializeField] [Range(0.1f,50.0f)]  private float _maximumDistanceFromTouchToToken = 10.0f;
    
    [SerializeField] private SwipeToken _tokenPrefab;
    [SerializeField] private SwipeToken[] _swappedTokensThisMove = new SwipeToken[2];
    /// <summary>
    /// Tokens that have matched this move due to any reason. 
    /// </summary>
    [SerializeField] private List<SwipeToken> _matchingTokensThisMove = new List<SwipeToken>();
    [SerializeField] private List<SwipeToken> _affectedTokensThisMove = new List<SwipeToken>();
    [SerializeField] private LayerMask _filter;
    [SerializeField] private GameState _state;
    private WizardMatchControls _controls;
    private Vector2 _touchScreenPosition;
    private Vector2 _pointLastTouched;
    private GameObject _selectedToken;
    private SwipeDirection _lastSwipedDirection; // unused so far
    void Awake()
    {
        InitializeBoard();
    }

    void Update()
    {
        switch(_state)
        {
            case GameState.READY :
                ReadyPhase();
                break;
            case GameState.CHECK_SWIPE :
                CheckSwipePhase();
                break;
            case GameState.MATCHING :
                MatchingPhase();
                break;
            case GameState.RETURN :
                ReturnUnmatchedSwipe();
                break;
        }
    }
    void MatchingPhase()
    {                
        // i don't remember why these are here lol.
        // the idea should be "start destroying all matching tokens this moment." that's it.
        // _swappedTokensThisMove should be reserved for check_swipe contexts only ideally.
        if (_swappedTokensThisMove[0])
            BreakTokensAndScore(_swappedTokensThisMove[0]);
        if (_swappedTokensThisMove[1])
            BreakTokensAndScore(_swappedTokensThisMove[1]);
        
        // if any token is not null or not idle, repeat until a change happens.

        foreach(SwipeToken token in playFieldTokens)
        {
            if (token && token.state == TokenState.IDLE)
                continue;
            else
                return;
        }

        _state = GameState.READY;
    }
    void ReadyPhase()
    {
        HandleInput();
    }
    void ReturnPhase()
    {
        ReturnUnmatchedSwipe();
    }
    void CheckSwipePhase()
    {
        CheckTokenForMatches(_swappedTokensThisMove[0]);
        CheckTokenForMatches(_swappedTokensThisMove[1]);
        if (_swappedTokensThisMove[0].matchType == MatchType.NO_MATCH && _swappedTokensThisMove[1].matchType == MatchType.NO_MATCH )
            _state = GameState.RETURN;
            
    }
    void ReturnUnmatchedSwipe()
    {
        if (_swappedTokensThisMove[0].state == TokenState.MOVING ||
            _swappedTokensThisMove[1].state == TokenState.MOVING)
            return;
        SwapTokenPositions(_swappedTokensThisMove[0],_swappedTokensThisMove[1]);
        _matchingTokensThisMove.Clear();
        _state = GameState.READY;
    }
    void BreakTokensAndScore(SwipeToken parentToken)
    {
        // temp function essentially. all bad.
        if (parentToken.state == TokenState.MOVING || !parentToken.matched) return;

        foreach(SwipeToken token in _matchingTokensThisMove)
        {
            if (token != parentToken)
                token.DestroyToken();
        }

        parentToken.DestroyToken();        
        _matchingTokensThisMove.Clear();
    }
    void HandleInput()
    {
        foreach(SwipeToken token in playFieldTokens)
        {
            // don't handle input if any token is currently in motion. 
            if (token.state == TokenState.MOVING)
                return;
        }
        // check to see if the point we've touched is actually a token or not.
        if (_controls.Touch.Tap.triggered)
        {
            _pointLastTouched = _touchScreenPosition;
            Ray ray = Camera.main.ScreenPointToRay(_touchScreenPosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
            
            if (!hit.collider) return; // if we didn't actually tap anything that was a token, return.
            
            _selectedToken = hit.collider.gameObject;
            _selectedToken.GetComponent<SwipeToken>().PlayAnimation("Pulsate");
        }
        
        Vector2 screenPosOfToken = _selectedToken ? Camera.main.WorldToScreenPoint(_selectedToken.transform.position) : _touchScreenPosition;
        Vector2 direction = (_touchScreenPosition - screenPosOfToken).normalized;
        
        bool upOrDown = Vector2.Dot(direction,Vector2.up) > 0? true : false;
        float angle = Vector2.Angle(Vector2.right,direction);

        // if we now have a valid selected token, and the position between our last tapped position
        // and our current screen position is greater than some threshold, commence a swipe.
        if (_selectedToken && Vector2.Distance(_pointLastTouched,_touchScreenPosition) > _minimumRequiredScreenSwipeDelta)
        {
            SwipeToken token = _selectedToken.GetComponent<SwipeToken>();
            SwipeToken neighborToken;
            // swipe right
            if (angle < 45.0f)
            {
                if (token.gridPosition.x < playFieldTokens.GetLength(0) - 1)
                {
                    neighborToken = playFieldTokens[token.gridPosition.x + 1, token.gridPosition.y];
                    _lastSwipedDirection = SwipeDirection.RIGHT;
                    SwapTokenPositions(token,neighborToken);
                }                
            }
            // swipe up
            else if (angle >= 45.0f && angle < 135.0f && upOrDown)
            {
                if (token.gridPosition.y > 0)
                {
                    neighborToken = playFieldTokens[token.gridPosition.x, token.gridPosition.y - 1];
                    _lastSwipedDirection = SwipeDirection.UP;
                    SwapTokenPositions(token,neighborToken);
                }
            }
            // swipe down
            else if (angle >= 45.0f && angle < 135.0f && !upOrDown)
            {
                if (token.gridPosition.y < playFieldTokens.GetLength(1) - 1)
                {
                    neighborToken = playFieldTokens[token.gridPosition.x, token.gridPosition.y + 1];
                    _lastSwipedDirection = SwipeDirection.DOWN;
                    SwapTokenPositions(token,neighborToken);
                }
            }
            // swipe left
            else if (angle >= 135.0f)
            {
                if (token.gridPosition.x > 0)
                {
                    neighborToken = playFieldTokens[token.gridPosition.x - 1, token.gridPosition.y];
                    _lastSwipedDirection = SwipeDirection.LEFT;
                    SwapTokenPositions(token,neighborToken);
                }
            }
            CancelGrabOfToken();
        }
    }
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
                Vector2 offset = startPosition + new Vector2(col * horizontalSpacing, -row * verticalSpacing);
                SwipeToken curTok = Instantiate(_tokenPrefab,offset,Quaternion.identity,gameObject.transform);
                curTok.swipeData = swipeScriptables[(int) Mathf.Floor(Random.Range(0,swipeScriptables.Count))];
                curTok.Initialize(new Vector2Int(col,row), GetComponent<GameBoard>());
                playFieldTokens[col,row] = curTok;

                Setup_CheckForUpwardMatches(curTok);
                Setup_CheckForLeftwardMatches(curTok);
            }
        }
        _controls = new WizardMatchControls();
        
        // assign actions to touch events
        _controls.Touch.ScreenPos.performed += context => { _touchScreenPosition = context.ReadValue<Vector2>(); };
        
        _controls.Touch.Tap.canceled += context => CancelGrabOfToken();
        _controls.Enable();

    }
    void CancelGrabOfToken()
    {
        if (_selectedToken)
            _selectedToken.GetComponent<SwipeToken>().PlayAnimation("Reset");
        _selectedToken = null;
    }
    short GenerateRandomColorDifferentFromNeighbors(SwipeToken A, SwipeToken B, SwipeToken C = null)
    {
        short colorA = A.realColor;
        short colorB = B.realColor;
        short colorC = C? C.realColor : (short) -1;

        while (true)
        {
            short newColor = (short) Mathf.Floor(Random.Range(0,swipeScriptables.Count));
            if (newColor != colorA && newColor != colorB && (colorC != -1 ? newColor != colorC : true))
                return newColor;
        }
    }
    void Setup_CheckForLeftwardMatches(SwipeToken token)
    {
        if (token.gridPosition.x <= 1)
            return;
        
        SwipeToken leftNeighbor = playFieldTokens[token.gridPosition.x - 1, token.gridPosition.y];
        SwipeToken leftmostNeighbor = playFieldTokens[token.gridPosition.x - 2, token.gridPosition.y];

        // first check. make sure that we aren't forming a line with our colors.
        bool firstMatch = leftNeighbor.realColor == token.realColor;
        bool secondMatch = leftNeighbor.realColor == leftmostNeighbor.realColor;

        if (firstMatch && secondMatch)
        {
            short newColor = GenerateRandomColorDifferentFromNeighbors(token,leftNeighbor,
                token.gridPosition.y > 0 ? playFieldTokens[token.gridPosition.x, token.gridPosition.y - 1] : null);
            
            token.ChangeColor(newColor);
            Setup_CheckForLeftwardMatches(leftNeighbor);
        }
        
    }
    void Setup_CheckForUpwardMatches(SwipeToken token)
    {
        if (token.gridPosition.y <= 1)
            return;
        SwipeToken upNeighbor = playFieldTokens[token.gridPosition.x, token.gridPosition.y - 1];
        SwipeToken upmostNeighbor = playFieldTokens[token.gridPosition.x, token.gridPosition.y - 2];

        // first check. make sure that we aren't forming a line with our colors.
        bool firstMatch = upNeighbor.realColor == token.realColor;
        bool secondMatch = upNeighbor.realColor == upmostNeighbor.realColor;

        if (firstMatch && secondMatch)
        {
            short newColor = GenerateRandomColorDifferentFromNeighbors(token,upNeighbor,
                token.gridPosition.x > 0 ? playFieldTokens[token.gridPosition.x - 1, token.gridPosition.y] : null);
            token.ChangeColor(newColor);
            Setup_CheckForUpwardMatches(upNeighbor);
        }
    }
    /// <summary>
    /// Swap the positions of tokens A and B both internally and externally, and prepare
    /// for the matching and waiting states. 
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    void SwapTokenPositions(SwipeToken A, SwipeToken B)
    {
        _state = GameState.CHECK_SWIPE;

        _swappedTokensThisMove[0] = A;
        _swappedTokensThisMove[1] = B;

        A.SwapToken(B);
        
        Vector2Int tempPosition = new Vector2Int(A.gridPosition.x, A.gridPosition.y);

        playFieldTokens[B.gridPosition.x,B.gridPosition.y] = A;
        playFieldTokens[A.gridPosition.x,A.gridPosition.y] = B;

        A.gridPosition = B.gridPosition;
        B.gridPosition = tempPosition;
    }
    /// <summary>
    /// Given a token, check if there are any matches in any of the 4 directions of any kind.
    /// </summary>
    /// <param name="token"></param>
    void CheckTokenForMatches(SwipeToken token)
    {
        // clear any neighbors that were there previously.
        
        token.likeHorizontalNeighbors.Clear();
        token.likeVerticalNeighbors.Clear();

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

        if (token.matchType == MatchType.NO_MATCH)
        {
            return;
        }

        _state = GameState.MATCHING;
        token.matched = true;

        if (xCount > yCount)
            token.likeVerticalNeighbors.Clear();
        else if (yCount > xCount)
            token.likeHorizontalNeighbors.Clear();

        foreach(SwipeToken neighborToken in token.likeHorizontalNeighbors)
            _matchingTokensThisMove.Add(neighborToken);
        foreach(SwipeToken neighborToken in token.likeVerticalNeighbors)
            _matchingTokensThisMove.Add(neighborToken);

        Debug.Log(token.matchType);

    }

}
