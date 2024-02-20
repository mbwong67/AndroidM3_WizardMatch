using System.Collections.Generic;
using proto;
using Unity.VisualScripting;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public List<SwipeScriptable> swipeScriptables = new List<SwipeScriptable>();

    public SwipeToken[,] playFieldTokens = new SwipeToken[7,5]; 

    private WizardMatchControls _controls;

    [SerializeField] [Range(0.1f,50.0f)]private float _minimumRequiredScreenSwipeDelta = 10.0f;
    
    [SerializeField] private SwipeToken _tokenPrefab;
    [SerializeField] private float _horizontalSpacing = 0.75f;
    [SerializeField] private float _verticalSpacing = 0.75f;
    [SerializeField] private Vector2 _startPosition = new Vector3();
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _filter;

    private Vector2 _touchScreenPosition;
    private Vector2 _touchScreenPositionLastFrame;

    private GameObject _selectedToken;

    private bool _isDraggingToken;


    void Awake()
    {
        InitializeBoard();
    }

    void Update()
    {
        HandleInput();
    }
    void LateUpdate()
    {
        _touchScreenPositionLastFrame = _touchScreenPosition;
    }

    void HandleInput()
    {
        // TODO : Make this a swiping motion instead. 

        // detect if our token has been touched.
        if (_controls.Touch.Tap.triggered)
        {
            Ray ray = _camera.ScreenPointToRay(_touchScreenPosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray,_filter);
            
            if (!hit.collider) return; // if we didn't actually tap anything that was a token, return.
            
            _isDraggingToken = true;
            _selectedToken = hit.collider.gameObject;

        }

        if (_isDraggingToken && _selectedToken)
        {
            Vector2 screenPosOfToken = _camera.WorldToScreenPoint(_selectedToken.transform.position);
            if (Vector2.Distance(screenPosOfToken,_touchScreenPosition) > _minimumRequiredScreenSwipeDelta)
            {
                Debug.Log("woah");
                SwipeToken token = _selectedToken.GetComponent<SwipeToken>();
                Vector2 direction = (_touchScreenPosition - screenPosOfToken).normalized;
                bool upOrDown = Vector2.Dot(direction,Vector2.up) > 0? true : false;
                float angle = Vector2.Angle(Vector2.right,direction);

                if (angle < 45.0f)
                {
                    SwipeToken rightNeighbor = token.xPosition < (short) playFieldTokens.GetLength(0) - 1 ? playFieldTokens[token.xPosition + 1, token.yPosition] : null;
                    Debug.Log(rightNeighbor == null);
                    if (rightNeighbor)
                    {
                        Debug.Log("Swap Right!");
                        token.SwapTokenPositions(rightNeighbor);
                    }
                    return;
                }
                if (angle >= 45.0f && angle < 135.0f && upOrDown)
                {
                    Debug.Log(token.upNeighbor == null);

                    if (token.upNeighbor)
                    {
                        Debug.Log("Swap Up!");
                        token.SwapTokenPositions(token.upNeighbor);
                    }
                    return;
                }
                if (angle >= 45.0f && angle < 135.0f && !upOrDown)
                {
                    SwipeToken downNeighbor = token.yPosition < (short) playFieldTokens.GetLength(1) - 1 ? playFieldTokens[token.xPosition,token.yPosition + 1] : null;
                    if (downNeighbor)
                    {
                        Debug.Log("Swap Down!");
                        token.SwapTokenPositions(downNeighbor);
                    }
                    return;
                }

                if (token.leftNeighbor)
                {
                    Debug.Log("Swap Left!");
                    token.SwapTokenPositions(token.leftNeighbor);
                }
            }
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

                Vector2 offset = _startPosition + new Vector2(col * _horizontalSpacing, -row * _verticalSpacing);
                SwipeToken curTok = Instantiate(_tokenPrefab,offset,Quaternion.identity,gameObject.transform);
                curTok.swipeData = swipeScriptables[(int) Mathf.Floor(Random.Range(0,5))];
                curTok.Initialize((short) col, (short) row);
                playFieldTokens[col,row] = curTok;

                if (row > 0)
                {
                    curTok.upNeighbor = playFieldTokens[col,row - 1];
                }
                if (col > 0)
                {
                    curTok.leftNeighbor = playFieldTokens[col - 1,row];
                }

                // DANGER !! WORLD'S SLOWEST ALGORITHM! Will inevitably have to optomize. For now, only good to about 15 x 15 board sizes!!!
                SetupCheckForUpwardMatches(curTok);
                SetupCheckForLeftwardMatches(curTok);
            }
        }
        _controls = new WizardMatchControls();
        // assign actions to touch events
        _controls.Touch.ScreenPos.performed += context => { _touchScreenPosition = context.ReadValue<Vector2>(); Debug.Log("touched!!"); };
        _controls.Touch.Tap.canceled += context => { _isDraggingToken = false; _selectedToken = null;};
        _controls.Enable();

    }
    void SetupCheckForLeftwardMatches(SwipeToken token)
    {
        if (token.xPosition <= 1)
            return;

        SwipeToken leftNeighbor = token.leftNeighbor;
        SwipeToken leftLeftNeighbor = leftNeighbor.leftNeighbor;

        if (
            token.realColor == leftNeighbor.realColor && leftNeighbor.realColor == leftLeftNeighbor.realColor
        )
        {
            while (true)
            {
                short newCol = (short) Mathf.Floor(Random.Range(0,5));
                SwipeToken rightNeighbor = token.xPosition < (short) playFieldTokens.GetLength(0) - 2 ? playFieldTokens[token.xPosition + 1, token.yPosition] : null;
                if (newCol != leftNeighbor.realColor)
                {
                    if (rightNeighbor && newCol == rightNeighbor.realColor)
                        rightNeighbor.ChangeColor(token.realColor);

                    token.ChangeColor(newCol);
                    SetupCheckForLeftwardMatches(leftNeighbor);
                    break;
                }
            }
        }
    }
    void SetupCheckForUpwardMatches(SwipeToken token)
    {
        if (token.yPosition <= 1)
            return;
        SwipeToken upNeighbor = token.upNeighbor;
        SwipeToken upUpNeighbor = upNeighbor.upNeighbor;

        // check if a coloumn has a match. if so, change the middle color
        if (
            token.realColor == upNeighbor.realColor && upNeighbor.realColor == upUpNeighbor.realColor
        )
        {
            while (true)
            {
                short newCol = (short) Mathf.Floor(Random.Range(0,5));
                SwipeToken downNeighbor = token.yPosition < (short) playFieldTokens.GetLength(1) - 2 ? playFieldTokens[token.xPosition, token.yPosition + 1] : null;

                if (newCol != upNeighbor.realColor)
                {
                    if (downNeighbor && newCol == downNeighbor.realColor)
                        downNeighbor.ChangeColor(token.realColor);

                    token.ChangeColor(newCol);
                    SetupCheckForUpwardMatches(upNeighbor);
                    break;
                }
            }
        }
    }

}
