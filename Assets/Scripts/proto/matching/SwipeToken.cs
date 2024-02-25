using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WizardMatch;

namespace proto
{
    
    // game object that the player interacts with that holds the data for it's associated swipe icon
    public class SwipeToken : MonoBehaviour
    {
        [SerializeField] public SwipeScriptable swipeData;
        [SerializeField] private Animator _animator;
        [SerializeField][Range(0.01f,0.5f)] private float _snappingDistance = 0.05f;
        [SerializeField] [Range(0.1f,5.0f)] private float _timeToMove  = 1.0f;
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
        private Vector2 _targetPosition;

        public bool matched = false;
        public bool isMoving = false;
        public MatchType matchType = MatchType.NO_MATCH;

        public short realColor = 0;

        public GameBoard gameBoard;
        public Vector2Int gridPosition = new Vector2Int();
        public List<SwipeToken> likeVerticalNeighbors = new List<SwipeToken>();
        public List<SwipeToken> likeHorizontalNeighbors = new List<SwipeToken>();

        public void Initialize(Vector2Int position, GameBoard gb)
        {
            if (!swipeData) // make sure the data isn't null before initializing
                return;

            gameBoard = gb;
            gridPosition = position;
            ChangeColor(swipeData.color);
        }
        public void ChangeColor(short color)
        {
            realColor = color;
            GetComponent<SpriteRenderer>().color = _colorAssociations[realColor];
        }
        public void PlayAnimation(string animation)
        {
            _animator.Play(animation);
        }
        void Awake()
        {
            _animator = GetComponent<Animator>();
            _targetPosition = transform.position;
        }
        void Update()
        {
            // just check to see if we are close or at our target position. if so, switch a flag to let the gameboard know we're done moving.
            CheckIfInPosition();
        }
        void CheckIfInPosition()
        {
            if (Vector2.Distance(transform.position,_targetPosition) < _snappingDistance)
            {
                transform.position = _targetPosition;
                isMoving = false;
            }
        }
        /// <summary>
        /// Move the tokens on the board. This has no logical component and is purely cosmetic.
        /// </summary>
        /// <param name="otherToken"></param>
        public void SwapToken(SwipeToken otherToken)
        {
            isMoving = true;
            Vector2 targetPositionA = new Vector2(
                gridPosition.x * gameBoard.horizontalSpacing, gridPosition.y * - gameBoard.verticalSpacing
            ) + gameBoard.startPosition + (Vector2) gameBoard.transform.position;
            
            Vector2 targetPositionB = new Vector2(
                otherToken.gridPosition.x * gameBoard.horizontalSpacing, otherToken.gridPosition.y * -gameBoard.verticalSpacing
            ) + gameBoard.startPosition + (Vector2) gameBoard.transform.position;

            StartCoroutine(otherToken.SmoothMove(_timeToMove,targetPositionA));
            StartCoroutine(SmoothMove(_timeToMove,targetPositionB));
        }

        // Recursively count the neighbors of similar color in a given direction. 
        public int CountNeighborsInCertainDirection(SwipeToken token, SwipeDirection direction, int neighborCount = 0)
        {
            
            SwipeToken neighbor;

            switch (direction)
            {
                case SwipeDirection.UP : 
                    if(token.gridPosition.y > 0 
                    && gameBoard.playFieldTokens[token.gridPosition.x,token.gridPosition.y - 1].realColor == token.realColor)
                    {
                        neighbor = gameBoard.playFieldTokens[token.gridPosition.x,token.gridPosition.y - 1];
                        likeVerticalNeighbors.Add(neighbor);
                        neighborCount = CountNeighborsInCertainDirection(neighbor,direction,neighborCount + 1);
                    }
                    break;
                case SwipeDirection.DOWN : 
                    if(token.gridPosition.y < gameBoard.playFieldTokens.GetLength(1) - 1
                    && gameBoard.playFieldTokens[token.gridPosition.x,token.gridPosition.y + 1].realColor == token.realColor)
                    {
                        neighbor = gameBoard.playFieldTokens[token.gridPosition.x,token.gridPosition.y + 1];
                        likeVerticalNeighbors.Add(neighbor);
                        neighborCount = CountNeighborsInCertainDirection(neighbor,direction,neighborCount + 1);
                    }
                    break;
                case SwipeDirection.LEFT : 
                    if(token.gridPosition.x > 0 
                    && gameBoard.playFieldTokens[token.gridPosition.x - 1,token.gridPosition.y].realColor == token.realColor)
                    {
                        neighbor = gameBoard.playFieldTokens[token.gridPosition.x - 1,token.gridPosition.y];
                        likeHorizontalNeighbors.Add(neighbor);
                        neighborCount = CountNeighborsInCertainDirection(neighbor,direction,neighborCount + 1);
                    }
                    break;
                case SwipeDirection.RIGHT : 
                    if(token.gridPosition.x < gameBoard.playFieldTokens.GetLength(0) - 1
                    && gameBoard.playFieldTokens[token.gridPosition.x + 1,token.gridPosition.y].realColor == token.realColor)
                    {
                        neighbor = gameBoard.playFieldTokens[token.gridPosition.x + 1,token.gridPosition.y];
                        likeHorizontalNeighbors.Add(neighbor);
                        neighborCount = CountNeighborsInCertainDirection(neighbor,direction,neighborCount + 1);
                    }
                    break;
            }

            return neighborCount;

        }
        IEnumerator SmoothMove (float time, Vector2 targetPosition)
        {
            Vector2 startingPos  = transform.position;
            float elapsedTime = 0;

            _targetPosition = targetPosition;
            isMoving = true;
            
            while (elapsedTime < time)
            {
                transform.position = Vector2.Lerp(startingPos, _targetPosition, elapsedTime / time);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}