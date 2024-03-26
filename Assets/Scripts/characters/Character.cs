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
    public enum CharacterState
    {
        IDLE,
        ATTACKING,
        DEFENDING, // unknown if needed.
        DAMAGE,
        DYING,
        OTHER,
        NONE
    }
    public static class CharacterEventManager 
    {
        public delegate void PerformAction(Character character);
        public static event PerformAction Performed = delegate{};
    }
    /// <summary>
    /// Big class to (hopefully) fully encompass all things a character can do. Contains methods for adjusting
    /// stats, playing and calling animation events, and affecting other characters.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    
    public class Character : MonoBehaviour , IAnimatable
    {
        /// <summary>
        /// After a move has been performed, call this event to carry out the 
        /// appropriate actions and animations.
        /// </summary>
        /// <param name="character"></param>
        public delegate void PerformAction(Character character);
        public static event PerformAction Performed = delegate{};
        /// <summary>
        /// After a move is complete, marked by the animation event associated with it, call this event. 
        /// </summary>
        /// <param name="animationName"></param>
        public delegate void OnCharacterAnimationFinished(string animationName);
        public event OnCharacterAnimationFinished OnCharacterAnimationFinish = delegate {};
        public CharacterData characterData;
        /// <summary>
        /// The character which is currently being targeted (if any);
        /// </summary>
        public Character targetCharacter;
        public CharacterAbility currentCharacterAbility = CharacterAbility.ATTACK;
        public CharacterState characterState;
        public Animator characterAnimator;

        public int atkModifier = 1;
        public int damageBonus = 0;
        public int hp = 0;
        public int atk = 0;
        public int def = 0;

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

            hp =  Mathf.Max(characterData.baseHP,hp);
            atk = Mathf.Max(characterData.baseATK,atk);
            def = Mathf.Max(characterData.baseDEF,def);
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

        public void AddToStat(int addend, string stat)
        {
            switch (stat)
            {
                case "HP" :
                    hp += addend;
                    if (hp >= characterData.baseHP)
                        hp = characterData.baseHP;
                    if (hp <= 0)
                        hp = 0;
                    break;
                case "ATK" :
                    atk += addend;
                    break;
                case "DEF" :
                    def += addend;
                    break;
                default :
                    Debug.LogWarning("Non-applicable stat : " + stat + "!! ");
                    break;
            }
        }
        /// <summary>
        /// Deal damage to target character's HP stat. 
        /// </summary>
        /// <param name="damage"></param>
        public void DealDamageToTarget(int damage)
        {
            PlayAnimation("Attack");
            targetCharacter.AddToStat(-damage,"HP");
        }
        public void ResetModifiers()
        {
            atkModifier = 1;
            damageBonus = 0;
        }
        public int GetDamageToDeal()
        {
            return (atk * atkModifier) + damageBonus;
        }

        public void PlayAnimation(string animation)
        {
            if (animation == "Attack" && characterData.characterType != CharacterType.PLAYER)
                characterAnimator.Play("EnemyAttack");
            else
                characterAnimator.Play(animation);
            
        }

        public void OnAnimationFinish(string animation)
        {
            switch(animation)
            {
                case "Attack" :
                    characterState = CharacterState.IDLE;
                    //SpawnHitbox();
                    break;
                case "Damage" :
                    if (hp <= 0)
                    {
                        PlayAnimation("Death");
                    }
                    else
                        characterState = CharacterState.IDLE;

                    break;
                case "Death" : 
                    // some other logic here to notify death.
                    Debug.Log("dead");
                    break;
                default :
                    break;
            }
            OnCharacterAnimationFinish(animation);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SpawnHitbox()
        {
            AttackHitbox obj = null;
            switch (currentCharacterAbility)
            {
                case CharacterAbility.ATTACK :
                    obj = Instantiate(characterData.attacks[0],transform.position,Quaternion.identity).GetComponent<AttackHitbox>();
                    break;
                case CharacterAbility.SUPPORT :
                    obj = Instantiate(characterData.attacks[1],transform.position,Quaternion.identity).GetComponent<AttackHitbox>();
                    break;
                case CharacterAbility.ULTIMATE :
                    obj = Instantiate(characterData.attacks[2],transform.position,Quaternion.identity).GetComponent<AttackHitbox>();
                    break;
                case CharacterAbility.OTHER :
                    break;
            }
            obj.dmg = GetDamageToDeal();
            obj.targetCharacter = targetCharacter;
        }

        public void OnAnimationBegin(string animation)
        {
            switch(animation) 
            {
                case "Attack" :
                    characterState = CharacterState.ATTACKING;
                    break;
                case "Death" :
                    characterState = CharacterState.DYING;
                    break;
                case "Damage" :
                    characterState = CharacterState.DAMAGE;
                    break;
            }
        }
    }
}