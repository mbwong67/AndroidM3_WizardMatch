using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WizardMatch
{
    // Holds enemies, their starting positions as well as player characters and their starting positions.
    // advances turns, manages score, tracks health, etc. relays information to the main game manager via 
    // it's public methods and properties. 
    public class PlayfieldCharacterManager : MonoBehaviour
    {
        public List<Character> friendlies = new List<Character>();
        public List<Character> enemies = new List<Character>();
        public List<CharacterData> spawnableEnemies = new List<CharacterData>();
        public Queue<Character> characterQueue = new Queue<Character>();

        public int matchCombo = 0;
        public int turn = 0;

        public Vector3[] startingFriendlyPositions = 
        {
            new Vector3(-1f,2.25f,0f),
            new Vector3(-1.75f,3.0f,0f),
            new Vector3(-2.25f,2f,0)        
        };

        public Vector3[] startingEnemyPositions = 
        {
            new Vector3(1f,2.25f,0f),
            new Vector3(1.75f,3.0f,0f),
            new Vector3(2.25f,2f,0)        
        };
        [SerializeField] private Character _currentCharacter;        

        void Awake()
        {
            List<Character> characters = new List<Character>();
            characters.AddRange(friendlies);
            characters.AddRange(enemies);
            foreach(Character character in characters)
                characterQueue.Enqueue(character);
            _currentCharacter = characterQueue.ElementAt(0);
        }
        void OnEnable()
        {
            MainGameManager.OnClear += OnClear;
        }
        void OnDisable()
        {
            MainGameManager.OnClear -= OnClear;
        }

        void Update()
        {
            
        }
        /// <summary>
        /// Called each time the board clears tokens. 
        /// </summary>
        void OnClear()
        {
            _currentCharacter.Engage();
            matchCombo++;
            Debug.Log("Cleared!");
        }
        public void AdvanceTurn()
        {
            matchCombo = 0;
            turn++;
            Character c = characterQueue.Dequeue();
            characterQueue.Enqueue(c);
        }
    }
}