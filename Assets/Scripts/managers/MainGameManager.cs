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
        
        [SerializeField] public GameBoard gameBoard;
        [SerializeField] public PlayfieldCharacterManager characterManager;
        
        [SerializeField] private int _currentLevel = 1;
        [SerializeField] private BlackScreenFader _fader;
        [SerializeField] private GameObject _selectedToken;
        [SerializeField] private SpecialAttack _specialAttack;
        [SerializeField] private SkipButton _skipButton;
        [SerializeField] [Range(0.1f,200.0f)] private float _minimumRequiredScreenSwipeDelta = 10.0f;

        private WizardToken[] _swipedTokensThisMove = new WizardToken[2];
        private Vector2 _touchScreenPosition;
        private Vector2 _pointLastTouched;
        private WizardMatchControls _controls;
        private Timer _generalTimer = new Timer(3.0f);
        void Awake()
        {
            Application.targetFrameRate = 60;
            MainGameState = GameState.NONE;
            _controls = new WizardMatchControls();            
            _controls.Touch.Tap.canceled += context => CancelGrab();
            _controls.Touch.ScreenPos.performed += context => { _touchScreenPosition = context.ReadValue<Vector2>(); };
        }
        void Initialize()
        {
            Debug.Log("woah");
            _controls.Enable();
            gameBoard.enabled = true;
            gameBoard.InitializeBoard();
            characterManager.InitializeCharacterManager();
            List<Character> chs = new List<Character>();
            chs.AddRange(characterManager.friendlies); chs.AddRange(characterManager.enemies); 
            
            foreach(Character character in chs)
                character.OnCharacterAnimationFinish += CharacterAnimationFinish;

            MainGameState = GameState.READY;
            _generalTimer = new Timer(3.0f);
            _generalTimer.OnTimerEnd += AdvanceLevel;

        }
        void CharacterAnimationFinish(string animation)
        {
            switch(animation)
            {
                case "Death" :
                    break;
            }
        }
        void OnEnable()
        {
            _fader.OnFadeIn += Initialize;
        }
        void OnDisable()
        {
            _fader.OnFadeIn -= Initialize;
            characterManager.currentActiveCharacter.OnCharacterAnimationFinish -= CharacterAnimationFinish;

        }
        bool CheckEnemiesForDeath()
        {
            foreach (Character c in characterManager.enemies)
            {
                if (c.characterState != CharacterState.DEAD)
                    return false;
            }
            foreach(Character c in characterManager.friendlies)
                        c.PlayAnimation("Win");
            return true;
        }
        void Update()
        {
            switch(MainGameState)
            {
                case GameState.READY :
                    HandleInput();
                    
                    _skipButton.gameObject.SetActive(true);

                    if (CheckEnemiesForDeath())
                    {
                        MainGameState = GameState.WIN;
                    }
                    break;
                case GameState.CHECK_SWIPE :
                    CheckSwipe();
                    break;
                case GameState.RETURN : 
                    WaitUntilSwipedTokensStop();
                    break;
                case GameState.MATCHING : 

                    _skipButton.gameObject.SetActive(false);
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
                        
                        Character cur = characterManager.currentActiveCharacter;
                        DetermineDamageModifiers(cur);
                        
                        _specialAttack.currentCharge += cur.GetDamageToDeal();

                        cur.OnCharacterAnimationFinish += OnAttackFinish;
                        characterManager.Execute();
                    }
                    break;
                // dead state. must change from outside sources. 
                case GameState.FRIENDLY_ATTACKING :
                case GameState.ENEMY_ATTACKING :
                {
                    
                    _skipButton.gameObject.SetActive(false);
                    break;
                }
                case GameState.WIN : 
                {
                    _generalTimer.Tick(Time.deltaTime);
                    break;
                }

                // wait until all characters are still and the board is stationary. at this point, resume to READY.
                case GameState.WAIT_GENERAL :
                    if (characterManager.AllCharactersAreStill() && gameBoard.boardIsStill)
                    {

                        gameBoard.matchCombo = 0;
                        gameBoard.tokenCombo = 0;
                        gameBoard.specialTokenModifier = 1;

                        characterManager.AdvanceTurn();
                        if (characterManager.currentActiveCharacter.characterData.characterType == CharacterType.PLAYER)
                            MainGameState = GameState.READY;
                        else
                            MainGameState = GameState.ENEMY_TURN;

                    }
                    if (CheckEnemiesForDeath())
                        MainGameState = GameState.WIN;
                    break;
                // for when the enemy needs to attack. 
                case GameState.ENEMY_TURN : 

                    if (characterManager.currentActiveCharacter.hp <= 0)
                        return;
                    characterManager.Execute();
                    characterManager.currentActiveCharacter.OnCharacterAnimationFinish += OnAttackFinish;

                    MainGameState = GameState.ENEMY_ATTACKING;
                    break;
                case GameState.NONE :
                    break;

            }
        }

        void DetermineDamageModifiers(Character c)
        {
            int combo = gameBoard.matchCombo - 1;
            int finalComboBonus = 0;

            if (combo >= 1 && combo < 2)
                finalComboBonus = 1;
            else if (combo >= 2 && combo < 5)
                finalComboBonus += 2;
            else if (combo >= 5)
                finalComboBonus += 3;

            int tokenCount = gameBoard.tokenCombo;
            int finalTokenBonus = 0;

            if (tokenCount >= 3 && tokenCount < 6)
                finalTokenBonus = 1;
            
            
            if (tokenCount >= 6 && tokenCount < 9)
                finalTokenBonus = 2;
            else if (tokenCount >= 9 && tokenCount < 15)
                finalTokenBonus = 3;
            else if (tokenCount >= 15 && tokenCount < 21)
                finalTokenBonus = 5;
            else if (tokenCount >= 21)
                finalTokenBonus = 7;
            

            c.comboBonus = finalComboBonus;
            c.tokenBonus = finalTokenBonus;
            c.atkModifier = gameBoard.specialTokenModifier;
        }
        /// <summary>
        /// Prep active character's damage numbers, break tokens on board. 
        /// </summary>
        void BreakAndScore()
        {
            OnClear();
            
            gameBoard.BreakAndScore();
            

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
            bool touchingSpecialAttack = false;
            bool touchingSkipButton = false;
            // check to see if the point we've touched is actually a token or not.
            if (_controls.Touch.Tap.triggered)
            {
                OnTouch();
                _pointLastTouched = _touchScreenPosition;
                Ray ray = Camera.main.ScreenPointToRay(_touchScreenPosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
                
                if (!hit.collider) return; // if we didn't actually tap anything that was a token, return.
                


                int magicNum = 1 << hit.collider.gameObject.layer;
                int specialAttackMagicNum = 1 << _specialAttack.gameObject.layer;
                int skipButtonMagicNum = 1 << _skipButton.gameObject.layer;

                if (magicNum == specialAttackMagicNum)
                {
                    touchingSpecialAttack = true;
                }
                else if (magicNum == skipButtonMagicNum)
                {
                    touchingSkipButton = true;
                }
                else
                {
                    _selectedToken = hit.collider.gameObject;
                    _selectedToken.GetComponent<WizardToken>().PlayAnimation("Pulsate");
                }
            }

            if (touchingSpecialAttack)
            {
                HandleSuperAttack(_specialAttack.gameObject);
                return;
            }
            if (touchingSkipButton)
            {
                HandleSkipButton(_skipButton.gameObject);
                return;
            }
            Vector2 screenPosOfToken = _selectedToken ? Camera.main.WorldToScreenPoint(_selectedToken.transform.position) : _touchScreenPosition;
            Vector2 direction = (_touchScreenPosition - screenPosOfToken).normalized;
            
            bool upOrDown = Vector2.Dot(direction,Vector2.up) > 0? true : false;
            float angle = Vector2.Angle(Vector2.right,direction);

            // if we now have a valid selected token, and the position between our last tapped position
            // and our current screen position is greater than some threshold, commence a swipe.
            if (_selectedToken && DetermineIfDistanceIsTooFarFromInitialTouch())
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


                CancelGrab();
                MainGameState = GameState.CHECK_SWIPE;
        }}
        
        bool DetermineIfDistanceIsTooFarFromInitialTouch()
        {
            return Vector2.Distance(_pointLastTouched,_touchScreenPosition) > _minimumRequiredScreenSwipeDelta;
        }

        void HandleSuperAttack(GameObject gameObject)
        {
            var obj = gameObject.GetComponent<SpecialAttack>();
            obj.PlayAnimation("Pulsate",1);
            obj.isTouched = true;
        }
        
        void HandleSkipButton(GameObject gameObject)
        {
            var obj = gameObject.GetComponent<SkipButton>();
            obj.PlayAnimation("Pulsate",1);
            _specialAttack.currentCharge += 5;
            obj.isTouched = true;
        }
        void CheckSwipe()
        {
            Character cur = characterManager.currentActiveCharacter;
            gameBoard.ResetTokens();
            // two simple checks to make sure our tokens are both there, and aren't moving. if so, don't run checks just yet. 
            if (!_swipedTokensThisMove[0]  || !_swipedTokensThisMove[1])
            {
                // in this specific case, the only way we could get here is if we cancel a swipe with no other token neighbor.
                // we must then return to READY
                MainGameState = GameState.READY;
                return;
            }
            if (_swipedTokensThisMove[0].tokenState == TokenState.MOVING || _swipedTokensThisMove[1].tokenState == TokenState.MOVING)
                return;
            

            // if this swipe isn't valid, return as soon as possible.
            gameBoard.CheckTokenForMatches(_swipedTokensThisMove[0]);
            gameBoard.CheckTokenForMatches(_swipedTokensThisMove[1]);
            if (_swipedTokensThisMove[0].matchType == MatchType.NO_MATCH && _swipedTokensThisMove[1].matchType == MatchType.NO_MATCH)
            {
                // if the current character currently doesn't have the ultimate charged, ignore however many have been matched and treat the first as 
                // if it does.
                if (cur.currentCharacterAbility == CharacterAbility.ULTIMATE)
                {
                    _swipedTokensThisMove[0].upgradeType = TokenUpgradeType.TURBO;
                    // also if it isn't in the running, add this token. 
                    if (!gameBoard.matchedTokens.Contains(_swipedTokensThisMove[0]))
                        gameBoard.matchedTokens.Add(_swipedTokensThisMove[0]);
                    MainGameState = GameState.MATCHING;
                    return;
                }

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
        void CancelGrab()
        {
            if (_selectedToken != null)
                _selectedToken.GetComponent<WizardToken>().PlayAnimation("Reset");
            
            if (_specialAttack.isTouched)
            {
                if(_specialAttack.currentCharge == _specialAttack.fullCharge)
                {
                    _specialAttack.PressButton();
                    var sandra = FindFirstObjectByType<Sandra>();
                    sandra.PlayAnimation("UltimateStart");
                }
                _specialAttack.isTouched = false;
                _specialAttack.PlayAnimation("Cancel",1);
            }
            if (_skipButton.isTouched)
            {
                _skipButton.PressButton();
                characterManager.AdvanceTurn();
                MainGameState = GameState.ENEMY_TURN;
                _skipButton.isTouched = false;
            }
            _selectedToken = null;
        }
        void AdvanceLevel()
        {
            // bad long and terrible.
            _currentLevel++;
            foreach(Character c in characterManager.enemies)
            {
                Destroy(c.gameObject);
            }

            int rand = Random.Range(0, _currentLevel);
            int whoToSpawn = 0;
            for (int i = 0; i < characterManager.levelInfo.spawnableEnemies.Count; i++)
            {
                var c = characterManager.levelInfo.spawnableEnemies[i].GetComponentInChildren<Character>();
                if (c.characterData.characterLevel >= rand)
                {
                    whoToSpawn = i;
                }
            }
            characterManager.enemies.Clear();
            var spawn = Instantiate(characterManager.levelInfo.spawnableEnemies[whoToSpawn],characterManager.enemySpawnPosition,Quaternion.identity);
            characterManager.enemies.Add(spawn.GetComponentInChildren<Character>());

            _generalTimer.SetTimer(3.0f);
            
            MainGameState = GameState.READY;

            characterManager.InitializeCharacterQueue();

            var sandra = FindFirstObjectByType<Sandra>();
            sandra.PlayAnimation("Idle");
            sandra.targetCharacter = spawn.GetComponentInChildren<Character>();

        }
    }
}