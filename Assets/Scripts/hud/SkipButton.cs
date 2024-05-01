using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace WizardMatch
{
    public class SkipButton : MonoBehaviour, IAnimatable
    {
        [SerializeField] public bool isTouched = false;
        [SerializeField] Animator _animator;
        [SerializeField] BoxCollider2D _boxCol;
        public void OnAnimationBegin(string animation)
        {
            throw new System.NotImplementedException();
        }

        public void OnAnimationFinish(string animation)
        {
            throw new System.NotImplementedException();
        }

        public void PlayAnimation(string animation, int layer = -1)
        {
            _animator.Play(animation);
        }
        public void PressButton()
        {
            _animator.Play("Cancel");
        }
    }
}