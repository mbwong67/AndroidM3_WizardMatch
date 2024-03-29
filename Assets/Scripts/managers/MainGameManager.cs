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
        public static event MatchCleared OnClear = delegate{};

        public delegate void Touched();
        public static event Touched OnTouch = delegate{};

        public delegate void Released();
        public static event Released OnRelease = delegate{};
        public GameState MainGameState;
        
        [SerializeField] private BlackScreenFader _fader;
        [SerializeField] public GameBoard gameBoard;
        [SerializeField] public PlayfieldCharacterManager characterManager;
        
        [SerializeField] private GameObject _selectedToken;
        [SerializeField] [Range(0.1f,200.0f)] private float _minimumRequiredScreenSwipeDelta = 10.0f;

        private WizardToken[] _swipedTokensThisMove = new WizardToken[2];
        private Vector2 _touchScreenPosition;
        private Vector2 _pointLastTouched;
        private WizardMatchControls _controls;
        void Awake()
        {
            Application.targetFrameRate = 60;
            MainGameState = GameState.READY;
            _controls = new WizardMatchControls();            
            _controls.Touch.Tap.canceled += context => CancelGrabOfToken();
            _controls.Touch.ScreenPos.performed += context => { _touchScreenPosition = context.ReadValue<Vector2>(); };
        }
        void Initialize()
        {
            _controls.Enable();
            gameBoard.enabled = true;
            gameBoard.InitializeBoard();
            characterManager.InitializeCharacterManager();
        }
        void OnEnable()
        {
            _fader.OnFadeIn += Initialize;
        }
        void OnDisable()
        {
            _fader.OnFadeIn -= Initialize;
        }
        // may not be needed.
        void TestEngage(Character character)
        {
            character.PlayAnimation("Attack");
        }
        void Update()
        {
            switch(MainGameState)
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
                    BreakAndScore();
                    MainGameState = GameState.WAIT_FOR_CASCADE;
                    break;
                case GameState.WAIT_FOR_CASCADE : 
                    WaitUntilTokenState(TokenState.IDLE,GameState.CASCADE);
                    break;
                case GameState.CASCADE :
                    if (gameBoard.boardIsStill)
                        gameBoard.Cascade();
                    WaitUntilTokenState(TokenState.IDLE,GameState.FINAL_CHECK_BEFORE_ATTACK);
                    break;
                // the last check on the board before moving to the player's attack move. 
                case GameState.FINAL_CHECK_BEFORE_ATTACK :
                    
                    if (gameBoard.CheckWholeBoardForMatches())
                    {
                        MainGameState = GameState.CASCADE;
                    }
                    // board is finally still and there are no more matches. prepare for next turn.
                    else
                    {
                        MainGameState = GameState.FRIENDLY_ATTACKING;
                        characterManager.currentActiveCharacter.OnCharacterAnimationFinish += OnAttackFinish;

                        characterManager.Execute();
                        gameBoard.specialTokenModifier = 1;
                    }
                    break;
                // dead state. must change from outside sources. 
                case GameState.FRIENDLY_ATTACKING :
                case GameState.ENEMY_ATTACKING :
                {
                    break;
                }

                // wait until all characters are still and the board is stationary. at this point, resume to READY.
                case GameState.WAIT_GENERAL :
                    if (characterManager.AllCharactersAreStill() && gameBoard.boardIsStill)
                    {
                        if (characterManager.currentActiveCharacter.characterData.characterType == CharacterType.ENEMY)
                            MainGameState = GameState.ENEMY_TURN;
                        else
                            MainGameState = GameState.READY;
                    }
                    break;
                // for when the enemy needs to attack. 
                case GameState.ENEMY_TURN : 

                    Debug.Log("enemy turn!!!");
                    if (characterManager.currentActiveCharacter.hp <= 0)
                        return;
                    characterManager.Execute();
                    characterManager.currentActiveCharacter.OnCharacterAnimationFinish += OnAttackFinish;

                    MainGameState = GameState.ENEMY_ATTACKING;
                    break;

            }
        }

        /// <summary>
        /// Prep active character's damage numbers, break tokens on board. 
        /// </summary>
        void BreakAndScore()
        {
            OnClear();
            
            gameBoard.BreakAndScore();
            
            Character cur = characterManager.currentActiveCharacter;
            cur.atkModifier = gameBoard.specialTokenModifier;
            cur.damageBonus = characterManager.matchCombo - 1;
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
            MainGameState = transitionState;
            gameBoard.ResetTokens();
        }
        void OnAttackFinish(string animation)
        {
            MainGameState = GameState.WAIT_GENERAL;
            // Unsubscribe this event to this method until needed to be invoked again. 
            characterManager.AdvanceTurn();
            characterManager.currentActiveCharacter.OnCharacterAnimationFinish -= OnAttackFinish;

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
                OnTouch();
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
                MainGameState = GameState.CHECK_SWIPE;
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
                MainGameState = GameState.RETURN;
                _swipedTokensThisMove[0].SwapTokenPositions(_swipedTokensThisMove[0],_swipedTokensThisMove[1]);
                return;
            }

            MainGameState = GameState.MATCHING;

        }
        void WaitUntilSwipedTokensStop(GameState stateIfSoIsTrue = GameState.READY)
        {
            if (_swipedTokensThisMove[0].tokenState == TokenState.IDLE && _swipedTokensThisMove[1].tokenState == TokenState.IDLE)
            {
                MainGameState = stateIfSoIsTrue;
            }
        }
        void CancelGrabOfToken()
        {
            if (_selectedToken)
                _selectedToken.GetComponent<WizardToken>().PlayAnimation("Reset");
            _selectedToken = null;

            OnRelease();
        }
    }
}