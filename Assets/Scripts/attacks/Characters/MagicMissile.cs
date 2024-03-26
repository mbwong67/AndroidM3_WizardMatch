using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardMatch;

namespace WizardMatch
{
    public class MagicMissile : AttackHitbox
    {
        [SerializeField] public GameObject projectile;
        private Vector3 _startPosition;
        Timer _timer = new Timer(0.5f);

        void Awake()
        {
            // if for some reason the projectile never makes it to the target hit box, kill this hitbox. 
            _timer.OnTimerEnd += KillHitBox;
            _startPosition = projectile.transform.position;
        }
        void Update()
        {
            _timer.Tick(Time.deltaTime);
            Move();
            CheckBoxBounds();
        }
        void Move()
        {
            projectile.transform.position = Vector2.Lerp(targetCharacter.transform.position,_startPosition,_timer.RemaingSeconds/_timer.MaxDuration);
        }
        void CheckBoxBounds()
        {
            var col = Physics2D.OverlapBox(projectile.transform.position,_boxCol.size,0,_filter);
            if (col && col.GetComponent<Character>() == targetCharacter)
            {
                DealDamage();
                KillHitBox();
            }
        }
        void KillHitBox()
        {
            _timer.OnTimerEnd -= KillHitBox;
            Destroy(gameObject);
        }
    }
}