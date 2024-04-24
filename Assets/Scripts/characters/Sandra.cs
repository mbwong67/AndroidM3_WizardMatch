using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardMatch
{
    /// <summary>
    /// Simple Extension specifically for Sandra to handle the problem of multiple moves outside of just attacking.
    /// </summary>
    public class Sandra : Character
    {
        new void Awake()
        {
            Debug.Log("Sandra Awake");
            defaultLayer = 1;
            base.Awake();
        }
        void Update()
        {
            MonitorState();
        }
        void MonitorState()
        {
            switch (currentCharacterAbility)
            {
                case CharacterAbility.ATTACK :
                    characterAnimator.SetLayerWeight(1,1);
                    characterAnimator.SetLayerWeight(2,0);
                    defaultLayer = 1;
                    break;
                case CharacterAbility.ULTIMATE :
                    characterAnimator.SetLayerWeight(1,0);
                    characterAnimator.SetLayerWeight(2,1);
                    defaultLayer = 2;
                    break;
            }
        }
        public void ChangeCharacterAbility(CharacterAbility characterAbility)
        {
            currentCharacterAbility = characterAbility;
        }
    }
}