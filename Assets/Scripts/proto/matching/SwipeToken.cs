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
        [SerializeField] [Range(0.1f,5.0f)] private float _timeToMove  = 1.0f;

        public bool matched;
        public SwipeToken leftNeighbor;
        public SwipeToken upNeighbor;

        public short realColor = 0;

        public Vector2Int gridPosition = new Vector2Int();
        private WizardMatchControls _controls;

        // TODO : this abstraction sucks. tie more closesly to playFieldTokens[x,y]

        public void Initialize(Vector2Int position)
        {
            
            if (!swipeData) // make sure the data isn't null before initializing
            {
                return;
            }

            gridPosition = position;
            ChangeColor(swipeData.color);
        }
        public void ChangeColor(short color)
        {
            realColor = color;
            switch(color)
            {   
                case 0 :
                    GetComponent<SpriteRenderer>().color = Color.red; // <-- each getcomponent<spriterenderer> here is just a test for now.
                    break;
                case 1 :
                    GetComponent<SpriteRenderer>().color = Color.green;
                    break;                    
                case 2 :
                    GetComponent<SpriteRenderer>().color = Color.blue;
                    break;                   
                case 3 :
                    GetComponent<SpriteRenderer>().color = Color.yellow;
                    break;
                case 4 :
                    GetComponent<SpriteRenderer>().color = (Color.red + Color.blue) / 2;
                    break;
            }
        }
        public void PlayAnimation(string animation)
        {
            _animator.Play(animation);
        }
        void Awake()
        {
            _controls = new WizardMatchControls();
            _controls.Enable();

            _animator = GetComponent<Animator>();
        }
        void Update()
        {

        }
        
        public void SwapToken(SwipeToken otherToken, SwipeDirection direction)
        {
            StartCoroutine(otherToken.SmoothMove(_timeToMove,transform.position));

            Vector2Int tempPosition = gridPosition;
            SwipeToken tempUpNeighbor = otherToken.upNeighbor;
            SwipeToken tempLeftNeighbor = otherToken.leftNeighbor;

            // SwipeToken tempToken = GetComponent<SwipeToken>();

            gridPosition = otherToken.gridPosition;
            otherToken.gridPosition = tempPosition;

            switch(direction)
            {
                case SwipeDirection.UP :

                    otherToken.leftNeighbor = leftNeighbor;

                    leftNeighbor = tempLeftNeighbor;
                    upNeighbor = tempUpNeighbor;

                    otherToken.upNeighbor = GetComponent<SwipeToken>();

                    break;
                case SwipeDirection.DOWN :

                    otherToken.upNeighbor = upNeighbor;
                    upNeighbor = otherToken;
                    
                    otherToken.leftNeighbor = leftNeighbor;

                    leftNeighbor = tempLeftNeighbor;

                    break;
                case SwipeDirection.LEFT :

                    otherToken.upNeighbor = upNeighbor;
                    leftNeighbor = tempLeftNeighbor;
                    upNeighbor = tempUpNeighbor;

                    otherToken.leftNeighbor = GetComponent<SwipeToken>();

                    break;
                case SwipeDirection.RIGHT :


                    otherToken.upNeighbor = upNeighbor;
                    
                    otherToken.leftNeighbor = leftNeighbor;
                    leftNeighbor = otherToken;

                    upNeighbor = tempUpNeighbor;

                    break;
            }

            StartCoroutine(SmoothMove(_timeToMove,otherToken.transform.position));
        }
        IEnumerator SmoothMove (float time, Vector2 targetPosition)
        {
            Vector2 startingPos  = transform.position;
            float elapsedTime = 0;
            while (elapsedTime < time)
            {
                transform.position = Vector2.Lerp(startingPos, targetPosition, (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}