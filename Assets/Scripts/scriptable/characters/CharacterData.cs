using System.Collections.Generic;
using UnityEngine;

namespace WizardMatch
{
    [CreateAssetMenu(fileName = "Character", menuName = "WizardMatch/CharacterData",order = 1)]
    public class CharacterData : ScriptableObject
    {
        public Element CharacterElement;
        public CharacterType characterType;
        /// <summary>
        /// List of attacks for this character to perform. Enemies should generally only have the 1 attack, but there are
        /// others for the main playable characters. 
        /// [0] = Primary attack
        /// [1] = Support ability
        /// [2] = Ultimate ability
        /// </summary>
        public List<GameObject> attacks;
        public string characterName = "Default";
        public int characterLevel = 0;
        public int baseHP = 0;
        public int baseATK = 0;
        public int baseDEF = 0;
    }
}