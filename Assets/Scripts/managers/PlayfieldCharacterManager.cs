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
        public LevelInfo levelInfo;

        [SerializeField] private List<GameObject> characterCards = new List<GameObject>();

        public int turn = 0;


        void Awake()
        {
            SpawnCharacters();
        }
        void Update()
        {
            
        }
        public void SpawnCharacters()
        {
            if (levelInfo.spawnPositions.Count != levelInfo.spawnableCharacters.Count)
            {
                Debug.LogError("ERROR : Spawn List " + levelInfo.SpawnlistName + " has incongruent counts for positions and characters!");
            }

            for(int i = 0; i < levelInfo.spawnableCharacters.Count; i++)
            {
                GameObject obj = Instantiate(levelInfo.spawnableCharacters[i],levelInfo.spawnPositions[i],Quaternion.identity);
                Character cha = obj.GetComponentInChildren<Character>();
                if (cha.characterData.characterType == CharacterType.PLAYER)
                {
                    friendlies.Add(cha);
                }
                else
                {
                    enemies.Add(cha);
                }
            }
        }
        public void InitializeCharacterManager()
        {
            List<Character> characters = new List<Character>();
            characters.AddRange(friendlies);
            characters.AddRange(enemies);
            foreach(Character character in characters)
                characterQueue.Add(character);
            AdvanceTurn();
            currentActiveCharacter.targetCharacter = FindTargetCharacter(enemies);

            foreach(GameObject card in characterCards)
            {
                card.GetComponentInChildren<HudEnemyHealthBar>().character = friendlies[0];
                card.GetComponent<GenericFader>().StartFade();
                card.SetActive(true);
                
            }

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
        /// Execute the attack animation for the current active character.
        /// </summary>
        public void Execute()
        {
            currentActiveCharacter.PlayAnimation("Attack");
            currentActiveCharacter.Engage();
        }
        /// <summary>
        /// Advance to next turn and reset state. 
        /// </summary>
        public void AdvanceTurn()
        {
            // temp
            if (characterQueue.Count < 2)
                return;
            if (currentActiveCharacter)
                currentActiveCharacter.ResetModifiers();
            turn++;

            currentActiveCharacter = characterQueue.ElementAt(0);

            Character pushBack = characterQueue[0];
            characterQueue.RemoveAt(0);
            characterQueue.Add(pushBack);

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