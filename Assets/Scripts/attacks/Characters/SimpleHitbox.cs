using UnityEngine;

namespace WizardMatch
{
    public class SimpleHitbox : AttackHitbox
    {

        void Awake()
        {
            
        }
        void Update()
        {
            transform.position = targetCharacter.transform.position;
            CheckBoxBounds();
        }
        void CheckBoxBounds()
        {
            var col = Physics2D.OverlapBox(transform.position,_boxCol.size,0,_filter);
            if (col && col.GetComponent<Character>() == targetCharacter)
            {
                DealDamage();
                KillHitBox();
            }
        }
        void KillHitBox()
        {
            Destroy(gameObject);
        }
    }
}