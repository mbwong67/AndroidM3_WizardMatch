using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardMatch
{
    // Class responsible for most token logic.
    // Calculates it's neighbors and updates it's position on the gameboard via a gameboard reference.
    public class WizardToken : MonoBehaviour, IAnimatable
    {
        [SerializeField] public SwipeScriptable swipeData;
        [SerializeField] public Vector2Int boardPosition;
        [SerializeField] public TokenState tokenState;
        [SerializeField] public SmoothMover mover;
        [SerializeField] public int realColor;
        [SerializeField] public float xSpacing;
        [SerializeField] public float ySpacing;
        [SerializeField] public bool matched = false;
        [SerializeField] public bool visited = false; // used for other matching algorithms
        [SerializeField] public bool shouldUpgrade = false;
        [SerializeField] public MatchType matchType;
        [SerializeField] public TokenUpgradeType upgradeType;

        [SerializeField] private Vector2 _startOffset;
        [SerializeField] private GameBoard _gameBoard;
        [SerializeField] private Animator _animator;

    #region Neighbor Tokens
        [SerializeField] public WizardToken northNeighbor;
        [SerializeField] public WizardToken southNeighbor;
        [SerializeField] public WizardToken eastNeighbor;
        [SerializeField] public WizardToken westNeighbor;
        [SerializeField] public List<WizardToken> likeVerticalNeighbors = new List<WizardToken>();
        [SerializeField] public List<WizardToken> likeHorizontalNeighbors = new List<WizardToken>();

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

        void OnEnable()
        {
            GameBoard.UpdateBoardPosition += UpdateBoardPosition;
        }
        void OnDisable()
        {
            GameBoard.UpdateBoardPosition -= UpdateBoardPosition;
        }
        void Update()
        {
            MonitorState();
        }
        void UpdateBoardPosition()
        {
            _gameBoard.playFieldTokens[boardPosition.x,boardPosition.y] = this;
        }
        void MonitorState()
        {
            if (mover.IsInPosition && tokenState != TokenState.DESTROYING)
            {
                tokenState = TokenState.IDLE;
                ForceMove(boardPosition);
            }
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
            // StartCoroutine(mover.MoveToPosition(targetPosition));
            mover.SetTargetPosition(targetPosition);
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

            // StartCoroutine(A.mover.MoveToPosition(A.transform.position,targetPositionForA));
            // StartCoroutine(B.mover.MoveToPosition(B.transform.position,targetPositionForB));
            A.mover.SetTargetPosition(targetPositionForA);
            B.mover.SetTargetPosition(targetPositionForB);

            A.RecalculateTokenNeighbors();
            B.RecalculateTokenNeighbors();
        }
        /// <summary>
        /// Destroy token if it isn't a special token.
        /// </summary>
        void DestroyToken()
        {
            _gameBoard.RepopulateBoard();
            
            if (!shouldUpgrade)
                Destroy(gameObject);
        }
        /// <summary>
        /// Physically move the token from position A to position B smoothly.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        public void ForceMove (Vector3 A, Vector3 B)
        {
            // StartCoroutine(mover.MoveToPosition(A,B));
            mover.SetTargetPosition(B);
        }
        /// <summary>
        /// Physically move the token to the specified board position smoothly. 
        /// </summary>
        /// <param name="boardPosition"></param>
        public void ForceMove (Vector2Int boardPosition)
        {
            this.boardPosition = boardPosition;
            Vector3 newPosition = ConvertBoardPositionToWorldPosition();
            // StartCoroutine(mover.MoveToPosition(transform.position,newPosition));
            mover.SetTargetPosition(newPosition);
        }
        /// <summary>
        /// Variant of force move in which the token instantly travels 
        /// towards the target destination instead of smoothly transitioning towards it. 
        /// </summary>
        /// <param name="boardPosition"></param>
        public void ForceMoveInstant(Vector2Int boardPosition)
        {
            this.boardPosition = boardPosition;
            mover.SetTargetPosition(ConvertBoardPositionToWorldPosition());
            transform.position = ConvertBoardPositionToWorldPosition();
        }
        /// <summary>
        /// Convert this token's board position into a usable world position relative to it's parent gameboard object.
        /// </summary>
        /// <returns></returns>
        Vector3 ConvertBoardPositionToWorldPosition()
        {
            float posX = xSpacing * boardPosition.x + _gameBoard.anchorPosition.x;
            float posY = ySpacing * -boardPosition.y + _gameBoard.anchorPosition.y;
            return new Vector3(posX,posY,0) + _gameBoard.transform.position;
        } 

        /// <summary>
        /// Physically moves token into an empty position on the board. Position MUST be empty, or will return early. Does not update neighbors of other tokens on the board.
        /// </summary>
        /// <param name="newBoardPosition"></param>
        public void MoveToEmptyBoardPosition(Vector2Int newBoardPosition)
        {
            if (_gameBoard.playFieldTokens[newBoardPosition.x,newBoardPosition.y])
                return;
            _gameBoard.playFieldTokens[boardPosition.x,boardPosition.y] = null;
            boardPosition = newBoardPosition;

            UpdateBoardPosition();

            Vector3 newPosition = ConvertBoardPositionToWorldPosition();

            ForceMove(transform.position,newPosition);
        }

        public void PlayAnimation(string animation, int layer = 0)
        {
            if (_animator != null)
                _animator.Play(animation);
            return;
        }
        /// <summary>
        /// Gather information about neighboring tokens.
        /// </summary>
        public void RecalculateTokenNeighbors()
        {
            westNeighbor = boardPosition.x > 0 ? _gameBoard.playFieldTokens[boardPosition.x - 1, boardPosition.y] : null;
            eastNeighbor = boardPosition.x < _gameBoard.playFieldTokens.GetLength(0) - 1 ? 
                _gameBoard.playFieldTokens[boardPosition.x + 1, boardPosition.y] : null;

            northNeighbor = boardPosition.y > 0 ? _gameBoard.playFieldTokens[boardPosition.x, boardPosition.y - 1] : null;
            southNeighbor = boardPosition.y < _gameBoard.playFieldTokens.GetLength(1) - 1 ? 
                _gameBoard.playFieldTokens[boardPosition.x, boardPosition.y + 1] : null;
        }

        /// <summary>
        /// Return a list of all neighbors of this token.
        /// </summary>
        /// <returns></returns>
        public List<WizardToken> GrabTokenNeighbors()
        {
            List<WizardToken> ret = new List<WizardToken>();

            if (westNeighbor)
                ret.Add(westNeighbor);
            if (eastNeighbor)
                ret.Add(eastNeighbor);
            if (northNeighbor)
                ret.Add(northNeighbor);
            if (southNeighbor)
                ret.Add(southNeighbor);

            return ret;
        }
        /// <summary>
        /// Recursively count the neighbors of similar color in a given direction. Used for checking for matches at swipe time
        /// for it's much simpler construction.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="direction"></param>
        /// <param name="neighborCount"></param>
        /// <returns></returns>
        public int CountNeighborsInCertainDirection(WizardToken token, SwipeDirection direction, int neighborCount = 0)
        {
            
            WizardToken neighbor;

            // check neighbors in the desired direction. 
            // if we encounter a token that has already been flagged as matched, we don't need to continue, as that is already
            // associated with a matching token.
            switch (direction)
            {
                case SwipeDirection.UP : 
                    if(token.boardPosition.y > 0 
                    && _gameBoard.playFieldTokens[token.boardPosition.x,token.boardPosition.y - 1].realColor == token.realColor)
                    {
                        neighbor = _gameBoard.playFieldTokens[token.boardPosition.x,token.boardPosition.y - 1];
                        if (neighbor.matched)
                            return neighborCount;
                        likeVerticalNeighbors.Add(neighbor);
                        neighborCount = CountNeighborsInCertainDirection(neighbor,direction,neighborCount + 1);
                    }
                    break;
                case SwipeDirection.DOWN : 
                    if(token.boardPosition.y < _gameBoard.playFieldTokens.GetLength(1) - 1
                    && _gameBoard.playFieldTokens[token.boardPosition.x,token.boardPosition.y + 1].realColor == token.realColor)
                    {
                        neighbor = _gameBoard.playFieldTokens[token.boardPosition.x,token.boardPosition.y + 1];
                        if (neighbor.matched)
                            return neighborCount;
                        likeVerticalNeighbors.Add(neighbor);
                        neighborCount = CountNeighborsInCertainDirection(neighbor,direction,neighborCount + 1);
                    }
                    break;
                case SwipeDirection.LEFT : 
                    if(token.boardPosition.x > 0 
                    && _gameBoard.playFieldTokens[token.boardPosition.x - 1,token.boardPosition.y].realColor == token.realColor)
                    {
                        neighbor = _gameBoard.playFieldTokens[token.boardPosition.x - 1,token.boardPosition.y];
                        if (neighbor.matched)
                            return neighborCount;
                        likeHorizontalNeighbors.Add(neighbor);
                        neighborCount = CountNeighborsInCertainDirection(neighbor,direction,neighborCount + 1);
                    }
                    break;
                case SwipeDirection.RIGHT : 
                    if(token.boardPosition.x < _gameBoard.playFieldTokens.GetLength(0) - 1
                    && _gameBoard.playFieldTokens[token.boardPosition.x + 1,token.boardPosition.y].realColor == token.realColor)
                    {

                        neighbor = _gameBoard.playFieldTokens[token.boardPosition.x + 1,token.boardPosition.y];
                        if (neighbor.matched)
                            return neighborCount;
                        likeHorizontalNeighbors.Add(neighbor);
                        neighborCount = CountNeighborsInCertainDirection(neighbor,direction,neighborCount + 1);
                    }
                    break;
            }
            return neighborCount;
        }

        public void OnAnimationFinish(string animation)
        {
        }

        public void OnAnimationBegin(string animation)
        {
        }
    }
}