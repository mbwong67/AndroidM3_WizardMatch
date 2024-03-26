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
        public List<Character> characterQueue = new List<Character>();
        public Character currentActiveCharacter;

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


        public void InitializeCharacterManager()
        {
            List<Character> characters = new List<Character>();
            characters.AddRange(friendlies);
            characters.AddRange(enemies);
            foreach(Character character in characters)
                characterQueue.Add(character);
            currentActiveCharacter = characterQueue.ElementAt(0);
            currentActiveCharacter.targetCharacter = FindTargetCharacter(enemies);

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
        /// If all characters are idle, return true.
        /// </summary>
        /// <returns></returns>
        public bool AllCharactersAreStill()
        {
            List<Character> characters = new List<Character>();
            characters.AddRange(friendlies);
            characters.AddRange(enemies);
            foreach(Character c in characters)
            {
                if (c.characterState != CharacterState.IDLE)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Called each time the board clears tokens. 
        /// </summary>
        void OnClear()
        {
            matchCombo++;
        }
        /// <summary>
        /// Execute the attack animation for the current active character.
        /// </summary>
        public void Execute()
        {
            Debug.Log(currentActiveCharacter.characterData.characterName + " has attacked!!");
            
            currentActiveCharacter.PlayAnimation("Attack");
            currentActiveCharacter.Engage();
        }
        /// <summary>
        /// Advance to next turn and reset state. 
        /// </summary>
        public void AdvanceTurn()
        {
            currentActiveCharacter.ResetModifiers();
            matchCombo = 0;
            turn++;
            Character newChar = characterQueue.ElementAt(characterQueue.Count - 1);
            characterQueue.RemoveAt(0);
            characterQueue.Add(currentActiveCharacter);
            currentActiveCharacter = newChar;

            switch (currentActiveCharacter.characterData.characterType)
            {
                case CharacterType.PLAYER :
                    currentActiveCharacter.targetCharacter = FindTargetCharacter(enemies);
                    break;
                case CharacterType.ENEMY : 
                    currentActiveCharacter.targetCharacter = FindTargetCharacter(friendlies);
                    break;
            }
        }

        /// <summary>
        /// For now, finds the character on the field with the lowest HP and makes them the target. 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Character FindTargetCharacter(List<Character> list)
        {
            var c = list[0];
            foreach(Character ch in list)
            {
                if (ch.hp < c.hp)
                    c = ch;
            }
            return c;
        }
    }
}