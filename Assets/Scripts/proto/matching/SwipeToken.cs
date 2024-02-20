using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardMatch;

namespace proto
{
    // game object that the player interacts with that holds the data for it's associated swipe icon
    public class SwipeToken : MonoBehaviour
    {
        [SerializeField] public SwipeScriptable swipeData;

        public bool matched;
        public SwipeToken leftNeighbor;
        public SwipeToken upNeighbor;

        public short realColor = 0;

        public short xPosition = 0;
        public short yPosition = 0;


        private WizardMatchControls _controls;

        public void Initialize(short x, short y)
        {
            
            if (!swipeData) // make sure the data isn't null before initializing
            {
                return;
            }
            xPosition = x;
            yPosition = y;
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
                    GetComponent<SpriteRenderer>().color = Color.red + Color.blue;
                    break;
            }
        }
        public void SwapTokenPositions(SwipeToken otherToken)
        {
            // temp
            Vector2 temp = transform.position;
            transform.position = otherToken.transform.position;
            transform.position = temp;
        }
        void Awake()
        {
            _controls = new WizardMatchControls();
            _controls.Enable();
        }
        void Update()
        {
        }
    }
}