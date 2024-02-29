using System.Collections;
using System.Collections.Generic;
using proto;
using UnityEngine;

namespace WizardMatch
{
    // Class responsible for user interaction and swiping. more than aesthetic.
    public class WizardToken : MonoBehaviour
    {
        [SerializeField] public SwipeScriptable swipeData;
        [SerializeField] public Vector2Int boardPosition;
        [SerializeField] public TokenState tokenState;
        [SerializeField] public SmoothMover mover;
        [SerializeField] public int realColor;
        [SerializeField] public float xSpacing; // unused
        [SerializeField] public float ySpacing; // unused
        [SerializeField] public bool matched { get; set;}

        [SerializeField] private Vector2 _startOffset;
        [SerializeField] private GameBoard _gameBoard;
        [SerializeField] private Animator _animator;

    #region Neighbor Tokens
        [SerializeField] public WizardToken northNeighbor;
        [SerializeField] public WizardToken southNeighbor;
        [SerializeField] public WizardToken eastNeighbor;
        [SerializeField] public WizardToken westNeighbor;
    #endregion
    
    #region Temporary
    [SerializeField] private Color[] _colorAssociations = 
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.white,    
        Color.magenta,
        Color.yellow,
        Color.gray,
        Color.black
    };
    
    #endregion

        void Update()
        {
            MonitorState();
        }
        void MonitorState()
        {
            if (GetComponent<SmoothMover>().IsInPosiiton())
                tokenState = TokenState.IDLE;
        }
        public void InitializeTokenAtStart(Vector2Int position, GameBoard gameBoard, float hor, float ver)
        {
            tokenState = TokenState.MOVING;
            boardPosition = position;
            _gameBoard = gameBoard;
            xSpacing = hor;
            ySpacing = ver;
            
            SetColor(swipeData.color);

            mover = GetComponent<SmoothMover>();
            Vector3 targetPosition = transform.position;
            Vector3 initialPos = transform.position + (Vector3)_startOffset + new Vector3(0, position.y + position.x);
            transform.position = initialPos;
            StartCoroutine(mover.MoveToPosition(initialPos,targetPosition));

            _animator = GetComponent<Animator>();
        }
        public void SetColor(int color)
        {
            realColor = color;
            GetComponent<SpriteRenderer>().color = _colorAssociations[color];
        }
        public void SwapTokenPositions(WizardToken A, WizardToken B)
        {
            A.tokenState = TokenState.MOVING;
            B.tokenState = TokenState.MOVING;

            _gameBoard.playFieldTokens[A.boardPosition.x,A.boardPosition.y] = B;
            _gameBoard.playFieldTokens[B.boardPosition.x,B.boardPosition.y] = A;

            Vector2Int tempPos = A.boardPosition;
            A.boardPosition = B.boardPosition;
            B.boardPosition = tempPos;


            Vector3 targetPositionForA = B.transform.position;
            Vector3 targetPositionForB = A.transform.position;

            StartCoroutine(A.mover.MoveToPosition(A.transform.position,targetPositionForA));
            StartCoroutine(B.mover.MoveToPosition(B.transform.position,targetPositionForB));
            

            A.GrabTokenNeighbors();
            B.GrabTokenNeighbors();
        }

        public void PlayAnimation(string animation)
        {
            _animator.Play(animation);
        }
        public void GrabTokenNeighbors()
        {
            westNeighbor = boardPosition.x > 0 ? _gameBoard.playFieldTokens[boardPosition.x - 1, boardPosition.y] : null;
            eastNeighbor = boardPosition.x < _gameBoard.playFieldTokens.GetLength(0) - 1 ? 
                _gameBoard.playFieldTokens[boardPosition.x + 1, boardPosition.y] : null;

            northNeighbor = boardPosition.y > 0 ? _gameBoard.playFieldTokens[boardPosition.x, boardPosition.y - 1] : null;
            southNeighbor = boardPosition.y < _gameBoard.playFieldTokens.GetLength(1) - 1 ? 
                _gameBoard.playFieldTokens[boardPosition.x, boardPosition.y + 1] : null;

        }
    }
}