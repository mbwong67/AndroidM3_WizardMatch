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
        public List<Character> characterQueue = new List<Character>();
        public Character currentActiveCharacter;
        public LevelInfo levelInfo;

        public Vector3 enemySpawnPosition;
        public Vector3 friendlySpawnPosition;

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
            
            GameObject obj = Instantiate(levelInfo.spawnableEnemies[0],enemySpawnPosition,Quaternion.identity);
            enemies.Add(obj.GetComponentInChildren<Character>());

            GameObject sandra = Instantiate(levelInfo.spawnablePlayers[0],friendlySpawnPosition,Quaternion.identity);
            friendlies.Add(sandra.GetComponentInChildren<Character>());

            currentActiveCharacter = sandra.GetComponentInChildren<Character>();

        }
        public void InitializeCharacterQueue()
        {
            characterQueue.Clear();
            List<Character> characters = new List<Character>();
            characters.AddRange(friendlies);
            characters.AddRange(enemies);
            foreach(Character character in characters)
                characterQueue.Add(character);
            currentActiveCharacter.targetCharacter = FindTargetCharacter(enemies);
        }
        public void InitializeCharacterManager()
        {
            InitializeCharacterQueue();
            AdvanceTurn();

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
        public Character FindTargetCharacter(List<Character> list)
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