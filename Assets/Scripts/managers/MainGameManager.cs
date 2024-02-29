using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardMatch
{
    public class MainGameManager : MonoBehaviour
    {
        [SerializeField] public GameBoard gameBoard;
        [SerializeField] private GameObject _selectedToken;
        [SerializeField] private GameState _gameState;
        [SerializeField] [Range(0.1f,200.0f)] private float _minimumRequiredScreenSwipeDelta = 10.0f;

        
        private Vector2 _touchScreenPosition;
        private Vector2 _pointLastTouched;
        private WizardMatchControls _controls;
        void Awake()
        {
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
            }
        }
        void HandleInput()
        {
            foreach(WizardToken token in gameBoard.playFieldTokens)
            {
                // don't handle input if any token is currently in motion. 
                if (token.tokenState == TokenState.MOVING)
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
                WizardToken neighborToken;
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
                CancelGrabOfToken();
        }}
        void CancelGrabOfToken()
        {
            if (_selectedToken)
                _selectedToken.GetComponent<WizardToken>().PlayAnimation("Reset");
            _selectedToken = null;
        }
    }
}