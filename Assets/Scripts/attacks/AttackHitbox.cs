using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardMatch
{
    /// <summary>
    /// Base class for all attack hitboxes.
    /// </summary>
    public class AttackHitbox : MonoBehaviour
    {
        public delegate void AttackComplete();
        public event AttackComplete OnAttackComplete = delegate{};
        [SerializeField] public int dmg = 0;
        [SerializeField] public Character targetCharacter;
        [SerializeField] protected LayerMask _filter;
        [SerializeField] protected BoxCollider2D _boxCol;

        public void DealDamage()
        {
            OnAttackComplete();
            targetCharacter.AddToStat(-dmg,"HP");
            targetCharacter.PlayAnimation("Damage");
        }
    }
}