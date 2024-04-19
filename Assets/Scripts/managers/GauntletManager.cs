using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardMatch
{
    public enum GauntletManagerState
    {
        READY,
        LEVEL_SELECTED,
        IDLE
    }
    public class GauntletManager : MonoBehaviour
    {
        private WizardMatchControls _controls;
        [SerializeField] private Vector2 _touchScreenPosition;
        [SerializeField] private Vector2 _pointLastTouched;

        void Awake()
        {
            _controls = new WizardMatchControls();
            _controls = new WizardMatchControls();            
            _controls.Touch.ScreenPos.performed += context => { _touchScreenPosition = context.ReadValue<Vector2>(); };
            _controls.Enable();
        }
        void Update()
        {
            HandleInput();
        }
        void HandleInput()
        {
            if (_controls.Touch.Tap.triggered)
            {
                OnTouch();
                _pointLastTouched = _touchScreenPosition;
                
                // RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
                
                // if (!hit.collider) return; // if we didn't actually tap anything that was a token, return.
                
            }
        }
        void OnTouch()
        {

        }
    }
}