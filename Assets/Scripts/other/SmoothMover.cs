using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WizardMatch
{
    public enum SmoothMoverType
    {
        NONE,
        EASE_OUT,
        EASE_IN
    }
    public class SmoothMover : MonoBehaviour
    {
        
        public bool IsInPosition { get; private set; }
        public SmoothMoverType moveType = SmoothMoverType.EASE_OUT;
        
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private Vector3 _startPosition;
        [SerializeField] private float t = 0.01f;
        private const float minMoveDistance = 0.01f;

        [SerializeField][Range(0.1f,50.0f)] private float _movementSpeed = 5.0f;

        void Awake()
        {
            _startPosition = transform.position;
        }
        void FixedUpdate()
        {
            if (!IsInPosition)
                Move();
            if (Vector3.Distance(transform.position,_targetPosition) < minMoveDistance)
            {
                transform.position = _targetPosition;
                IsInPosition = true;
                t = 0.01f;
            }
        }
        public void SetTargetPosition(Vector3 targetPosition)
        {
            IsInPosition = false;
            _targetPosition = targetPosition;
        }
        public void SetStartPosition(Vector3 startPosition)
        {
            _startPosition = startPosition;
        }
        void Move()
        {
            switch(moveType)
            {
                case SmoothMoverType.EASE_IN :
                    EaseIn();
                    break;
                case SmoothMoverType.EASE_OUT :
                    EaseOut();
                    break;
                default :
                    break;
            }
        }

        void EaseOut()
        {
            transform.position = Vector3.Lerp(transform.position,_targetPosition,Time.fixedDeltaTime * _movementSpeed);
        }
        void EaseIn()
        {
            t += t * Time.fixedDeltaTime * _movementSpeed;
            if (t >= 1.0f)
            {
                transform.position = _targetPosition;
                return;
            }
            transform.position = Vector3.Lerp(_startPosition,_targetPosition,t);
        }
    }
}