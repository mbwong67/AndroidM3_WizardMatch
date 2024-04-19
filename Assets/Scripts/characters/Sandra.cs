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

        [SerializeField] public bool UltimateAttackReady;

        void Update()
        {
            if (UltimateAttackReady)
            {
                
            }
        }
    }
}