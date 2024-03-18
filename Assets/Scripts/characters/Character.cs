using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardMatch
{
    public enum CharacterAbility 
    {
        ATTACK,
        SUPPORT,
        ULTIMATE,
        OTHER
    }
    public class Character : MonoBehaviour
    {
        public delegate void PerformAction(Character character);
        public static event PerformAction Performed;
        public CharacterData characterData;
        /// <summary>
        /// The character which is currently being targeted (if any);
        /// </summary>
        public Character targetCharacter;
        public CharacterAbility currentCharacterAbility;
        public Animator characterAnimator;
        public uint hp = 0;
        public uint atk = 0;
        public uint def = 0;

        void Awake()
        {
            if (!characterData)
            {
                Debug.LogError("ERROR : characterData for " + gameObject.name + " was never initialized! Aborting");
                Destroy(gameObject);
            }
            if (!characterAnimator)
            {
                TryGetComponent(out Animator a);
                characterAnimator = a;
            }
            
            
            // safeguard in case any of the stats were changed in the inspector, we 
            // still use those stats instead of the ones given by the data.

            hp = (uint) Mathf.Max(characterData.baseHP,hp);
            atk = (uint) Mathf.Max(characterData.baseATK,atk);
            def = (uint) Mathf.Max(characterData.baseDEF,def);
        }
        /// <summary>
        /// Invoke the "Performed" event, passing the character it was called from as argument.
        /// All information needed by the recieving party should be contained within this character
        /// prior to invoking. 
        /// </summary>
        public void Engage()
        {
            Performed(this);
        }
    }
}