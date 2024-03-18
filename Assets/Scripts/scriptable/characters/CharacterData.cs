using UnityEngine;

namespace WizardMatch
{
    [CreateAssetMenu(fileName = "Character", menuName = "WizardMatch/CharacterData",order = 1)]
    public class CharacterData : ScriptableObject
    {
        public Element CharacterElement;
        public string characterName = "Default";
        public int characterLevel = 0;
        public uint baseHP = 0;
        public uint baseATK = 0;
        public uint baseDEF = 0;
    }
}