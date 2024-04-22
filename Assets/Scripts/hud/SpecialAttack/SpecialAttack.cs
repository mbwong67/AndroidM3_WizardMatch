using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace WizardMatch
{
    public class SpecialAttack : MonoBehaviour, IAnimatable
    {
        [SerializeField] public int currentCharge;
        [SerializeField] public int fullCharge;
        // is very very very bad. this should never have been done. don't do this ever again
        // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        [SerializeField] public bool isTouched = false;
        [SerializeField] public GameObject specialButton;

        [SerializeField] public BoxCollider2D _specialButtonHitbox;
        [SerializeField] private Animator _animator;
        [SerializeField] private Color _loopColor;
        [SerializeField] private SpriteRenderer _specialButtonSprite;
        [SerializeField] private ScalingBar _scaleBar;

        private Timer _colorTimer = new Timer(2.0f);

        void Awake()
        {
            if (!_scaleBar)
            {
                Debug.LogError("ERROR : Special Attack UI " + gameObject.name + " does not have associated scaling bar component! Aborting...");
                Destroy(gameObject);
            }
            _scaleBar.maxValue = fullCharge;
            _scaleBar.value = currentCharge;

        }

        void Update()
        {
            MonitorState();
        }
        void MonitorState()
        {
            _scaleBar.value = currentCharge;
            if (_scaleBar.value >= (float) fullCharge)
            {
                _scaleBar.barColor = _scaleBar.defaultBarColor;
                _specialButtonSprite.color = _loopColor;
                _scaleBar.value = fullCharge;
                currentCharge = fullCharge;
                SpecialColorLoop();
            }
            else
            {
                _animator.Play("default");
                _scaleBar.barColor = _loopColor;
                _specialButtonSprite.color = _loopColor;
            }
            
        }
        void SpecialColorLoop()
        {
            _animator.Play("colorLoop");
        }
        public void PressButton()
        {
            if (currentCharge == fullCharge && isTouched)
            {
                _animator.Play("Cancel",1);
                currentCharge = 0;
            }
        }

        public void PlayAnimation(string animation, int layer = 0)
        {
            _animator.Play(animation,layer);
        }

        public void OnAnimationFinish(string animation)
        {
            throw new System.NotImplementedException();
        }

        public void OnAnimationBegin(string animation)
        {
            throw new System.NotImplementedException();
        }
    }
}