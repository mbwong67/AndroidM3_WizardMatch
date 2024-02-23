using System.Collections.Generic;
using proto;
using UnityEngine;
using WizardMatch;

public class GameBoard : MonoBehaviour
{
    public List<SwipeScriptable> swipeScriptables = new List<SwipeScriptable>();
    public SwipeToken[,] playFieldTokens = new SwipeToken[7,7]; 
    public float horizontalSpacing = 0.75f;
    public float verticalSpacing = 0.75f;
    public Vector2 startPosition = new Vector3();


    [SerializeField] [Range(0.1f,200.0f)] private float _minimumRequiredScreenSwipeDelta = 10.0f;
    [SerializeField] [Range(0.1f,50.0f)]  private float _maximumDistanceFromTouchToToken = 10.0f;
    
    [SerializeField] private SwipeToken _tokenPrefab;
    [SerializeField] private SwipeToken[] _swappedTokensThisMove = new SwipeToken[2];
    [SerializeField] private List<SwipeToken> _matchingTokensThisMove = new List<SwipeToken>();
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
                HandleInput();
                break;
            case GameState.WAIT : 
                CheckBoardForMatches(_swappedTokensThisMove[0]);
                CheckBoardForMatches(_swappedTokensThisMove[1]);
                break;
            case GameState.MATCHING :
                BreakTokensAndScore();
                break;
        }
    }

    void BreakTokensAndScore()
    {
        if (_swappedTokensThisMove[0].isMoving || _swappedTokensThisMove[1].isMoving) return;
        foreach(SwipeToken token in _matchingTokensThisMove)
            Destroy(token.gameObject);
        
        _matchingTokensThisMove.Clear();
    }
    void HandleInput()
    {
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
                curTok.swipeData = swipeScriptables[(int) Mathf.Floor(Random.Range(0,5))];
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
        // _isDraggingToken = false; // <-- unused
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
            Debug.Log("\nLeftward Change : \n X Position : " + token.gridPosition.x + "\nY Position : " + token.gridPosition.y + "\n");
            
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
            
            Debug.Log("\nUpward Change : \n X Position : " + token.gridPosition.x + "\nY Position : " + token.gridPosition.y + "\n");
            
            short newColor = GenerateRandomColorDifferentFromNeighbors(token,upNeighbor,
                token.gridPosition.x > 0 ? playFieldTokens[token.gridPosition.x - 1, token.gridPosition.y] : null);
            token.ChangeColor(newColor);
            Setup_CheckForUpwardMatches(upNeighbor);
        }
    }
    void SwapTokenPositions(SwipeToken A, SwipeToken B)
    {
        _state = GameState.WAIT;

        _swappedTokensThisMove[0] = A;
        _swappedTokensThisMove[1] = B;

        A.SwapToken(B);
        
        Vector2Int tempPosition = new Vector2Int(A.gridPosition.x, A.gridPosition.y);

        playFieldTokens[B.gridPosition.x,B.gridPosition.y] = A;
        playFieldTokens[A.gridPosition.x,A.gridPosition.y] = B;

        A.gridPosition = B.gridPosition;
        B.gridPosition = tempPosition;
    }
    void CheckBoardForMatches(SwipeToken token)
    {
        token.likeHorizontalNeighbors.Clear();
        token.likeVerticalNeighbors.Clear();
        int xCount = 
            token.CountNeighborsInCertainDirection(token,SwipeDirection.LEFT,0) + token.CountNeighborsInCertainDirection(token,SwipeDirection.RIGHT,0);
        int yCount = 
            token.CountNeighborsInCertainDirection(token,SwipeDirection.DOWN,0) + token.CountNeighborsInCertainDirection(token,SwipeDirection.UP,0);

        if (xCount >= 4 || yCount >= 4)
            token.matchType = MatchType.FIVE_IN_A_ROW;
        else if (xCount == 3 || yCount == 3)
        {
            if (xCount >= 2 && yCount >= 2)
                token.matchType = MatchType.CROSS;
            else
                token.matchType = MatchType.FOUR_IN_A_ROW;
        }
        else if (xCount == 2 || yCount == 2)
            token.matchType = MatchType.THREE_IN_A_ROW;
        else
            token.matchType = MatchType.NO_MATCH;

        if (token.matchType == MatchType.NO_MATCH) return;

        _state = GameState.MATCHING;
        token.matched = true;

        if (xCount > yCount)
        {
            token.likeVerticalNeighbors.Clear();
        }            
        else if (yCount > xCount)
        {
            token.likeHorizontalNeighbors.Clear();
        }

    }

}
