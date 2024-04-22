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
                    break;
                case CharacterAbility.ULTIMATE :
                    characterAnimator.SetLayerWeight(1,0);
                    characterAnimator.SetLayerWeight(2,1);
                    break;
            }
        }
    }
}