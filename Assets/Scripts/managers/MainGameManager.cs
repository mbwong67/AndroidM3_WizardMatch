using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardMatch
{
    // Handles the calling of methods and organization of objects within. 
    // Issues method calls to appropriate objects depending on state.
    // Also handles input.
    public class MainGameManager : MonoBehaviour
    {
        public delegate void MatchCleared();
        public event MatchCleared OnClear;

        [SerializeField] public GameBoard gameBoard;
        [SerializeField] private GameObject _selectedToken;
        [SerializeField] private GameState _gameState;
        [SerializeField] [Range(0.1f,200.0f)] private float _minimumRequiredScreenSwipeDelta = 10.0f;

        private WizardToken[] _swipedTokensThisMove = new WizardToken[2];
        private Vector2 _touchScreenPosition;
        private Vector2 _pointLastTouched;
        private WizardMatchControls _controls;
        void Awake()
        {
            Application.targetFrameRate = 60;
            _controls = new WizardMatchControls();            
            _controls.Touch.Tap.canceled += context => CancelGrabOfToken();
            _controls.Touch.ScreenPos.performed += context => { _touchScreenPosition = context.ReadValue<Vector2>(); };
            _controls.Enable();
        }

        void Update()
        {
            switch(_gameState)
            {
                case GameState.READY :
                    HandleInput();
                    break;
                case GameState.CHECK_SWIPE :
                    CheckSwipe();
                    break;
                case GameState.RETURN : 
                    WaitUntilSwipedTokensStop();
                    break;
                case GameState.MATCHING : 
                    gameBoard.BreakAndScore();
                    _gameState = GameState.WAIT;
                    break;
                case GameState.WAIT : 
                    WaitUntilTokenState(TokenState.IDLE,GameState.CASCADE);
                    break;
                case GameState.CASCADE :
                    if (gameBoard.boardIsStill)
                        gameBoard.Cascade();
                    WaitUntilTokenState(TokenState.IDLE,GameState.FINAL_CHECK);
                    break;
                case GameState.FINAL_CHECK :
                    
                    if (gameBoard.CheckWholeBoardForMatches())
                    {
                        _gameState = GameState.CASCADE;
                    }
                    else
                        _gameState = GameState.READY;
                    break;

            }
        }

        /// <summary>
        /// Cycle through each token and wait until all states are of the desired state. Then, transition to desired game state.
        /// </summary>
        /// <param name="desiredState"></param>
        /// <param name="transitionState"></param>
        void WaitUntilTokenState(TokenState desiredState, GameState transitionState)
        {
            for (int col = 0; col < gameBoard.playFieldTokens.GetLength(0); col++)
            {
                for (int row = 0; row < gameBoard.playFieldTokens.GetLength(1); row++)
                    if (!gameBoard.playFieldTokens[col,row] || gameBoard.playFieldTokens[col,row].tokenState != desiredState)
                        return;
            }
            _gameState = transitionState;
            gameBoard.ResetTokens();
        }
        void HandleInput()
        {

            foreach(WizardToken token in gameBoard.playFieldTokens)
            {
                // don't handle input if any token is currently in motion. 
                if (token && token.tokenState == TokenState.MOVING)
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
                _selectedToken.GetComponent<WizardToken>().PlayAnimation("Pulsate");
            }
            
            Vector2 screenPosOfToken = _selectedToken ? Camera.main.WorldToScreenPoint(_selectedToken.transform.position) : _touchScreenPosition;
            Vector2 direction = (_touchScreenPosition - screenPosOfToken).normalized;
            
            bool upOrDown = Vector2.Dot(direction,Vector2.up) > 0? true : false;
            float angle = Vector2.Angle(Vector2.right,direction);

            // if we now have a valid selected token, and the position between our last tapped position
            // and our current screen position is greater than some threshold, commence a swipe.
            if (_selectedToken && Vector2.Distance(_pointLastTouched,_touchScreenPosition) > _minimumRequiredScreenSwipeDelta)
            {
                WizardToken token = _selectedToken.GetComponent<WizardToken>();
                WizardToken neighborToken = null;
                // swipe right
                if (angle < 45.0f)
                {
                    if (token.boardPosition.x < gameBoard.playFieldTokens.GetLength(0) - 1)
                    {
                        neighborToken = gameBoard.playFieldTokens[token.boardPosition.x + 1, token.boardPosition.y];
                        token.SwapTokenPositions(token,neighborToken);
                    }                
                }
                // swipe up
                else if (angle >= 45.0f && angle < 135.0f && upOrDown)
                {
                    if (token.boardPosition.y > 0)
                    {
                        neighborToken = gameBoard.playFieldTokens[token.boardPosition.x, token.boardPosition.y - 1];
                        token.SwapTokenPositions(token,neighborToken);
                    }
                }
                // swipe down
                else if (angle >= 45.0f && angle < 135.0f && !upOrDown)
                {
                    if (token.boardPosition.y < gameBoard.playFieldTokens.GetLength(1) - 1)
                    {
                        neighborToken = gameBoard.playFieldTokens[token.boardPosition.x, token.boardPosition.y + 1];
                        token.SwapTokenPositions(token,neighborToken);
                    }
                }
                // swipe left
                else if (angle >= 135.0f)
                {
                    if (token.boardPosition.x > 0)
                    {
                        neighborToken = gameBoard.playFieldTokens[token.boardPosition.x - 1, token.boardPosition.y];
                        token.SwapTokenPositions(token,neighborToken);
                    }
                }
                _swipedTokensThisMove[0] = token;
                _swipedTokensThisMove[1] = neighborToken;

                CancelGrabOfToken();
                _gameState = GameState.CHECK_SWIPE;
        }}
        void CheckSwipe()
        {
            gameBoard.ResetTokens();
            // two simple checks to make sure our tokens are both there, and aren't moving. if so, don't run checks just yet. 
            if (!_swipedTokensThisMove[0]  || !_swipedTokensThisMove[1])
                return;
            if (_swipedTokensThisMove[0].tokenState == TokenState.MOVING || _swipedTokensThisMove[1].tokenState == TokenState.MOVING)
                return;
            
            gameBoard.CheckTokenForMatches(_swipedTokensThisMove[0]);
            gameBoard.CheckTokenForMatches(_swipedTokensThisMove[1]);

            // if this swipe isn't valid, return as soon as possible.
            if (_swipedTokensThisMove[0].matchType == MatchType.NO_MATCH && _swipedTokensThisMove[1].matchType == MatchType.NO_MATCH)
            {
                Debug.Log("returning");
                _gameState = GameState.RETURN;
                _swipedTokensThisMove[0].SwapTokenPositions(_swipedTokensThisMove[0],_swipedTokensThisMove[1]);
                return;
            }

            _gameState = GameState.MATCHING;

        }
        void WaitUntilSwipedTokensStop(GameState stateIfSoIsTrue = GameState.READY)
        {
            if (_swipedTokensThisMove[0].tokenState == TokenState.IDLE && _swipedTokensThisMove[1].tokenState == TokenState.IDLE)
            {
                _gameState = stateIfSoIsTrue;
            }
        }
        void CancelGrabOfToken()
        {
            if (_selectedToken)
                _selectedToken.GetComponent<WizardToken>().PlayAnimation("Reset");
            _selectedToken = null;
        }
    }
}