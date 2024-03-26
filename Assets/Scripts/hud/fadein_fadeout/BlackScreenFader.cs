using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardMatch
{
    /// <summary>
    /// Attach to pure black sprite to have a simple fade in -> fade out transition. 
    /// Has assignable events for whatever use case you wish to employ for the fader.
    /// </summary>
    public class BlackScreenFader : MonoBehaviour, IAnimatable
    {
        public delegate void FadeOut();
        public delegate void FadeIn();
        public event FadeOut OnFadeOut = delegate {};
        public event FadeIn OnFadeIn = delegate {};

        [SerializeField] private SpriteRenderer _blackSprite;
        [SerializeField] private Animator _animator;
        void Awake() 
        {
            _blackSprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
        }
        public void PlayAnimation(string animation)
        {
            _animator.Play(animation);
        }

        public void OnAnimationFinish(string animation)
        {
            OnFadeOut();
        }

        public void OnAnimationBegin(string animation)
        {
            OnFadeIn();
        }
    }
}