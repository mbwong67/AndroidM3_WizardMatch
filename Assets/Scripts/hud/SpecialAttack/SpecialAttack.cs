using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace WizardMatch
{
    public class SpecialAttack : MonoBehaviour
    {
        [SerializeField] public int currentCharge;
        [SerializeField] public int fullCharge;
        [SerializeField] public GameObject specialButton;
        [SerializeField] private Color _loopColor;
        [SerializeField] private SpriteRenderer _specialButtonSprite;
        [SerializeField] private ScalingBar _scaleBar;
        [SerializeField] private Animator _animator;

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
            currentCharge = 0;
        }

    }
}