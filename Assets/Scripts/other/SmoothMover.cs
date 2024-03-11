using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WizardMatch
{
    public class SmoothMover : MonoBehaviour
    {
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        [SerializeField][Range(0.1f,10.0f)] private float _movementSpeed = 5.0f;

        public IEnumerator MoveToPosition(Vector3 a, Vector3 b) 
        {
            _startPosition = a;
            _targetPosition = b;
            float step = (_movementSpeed / (a - b).magnitude) * Time.fixedDeltaTime;
            float radStep = 2 * Mathf.PI / step;
            float t = 0;
            float r = 0;
            while (t <= 1.0f) 
            {
                t += step; // Goes from 0 to 1, incrementing by step each time
                r += radStep;
                transform.position = Vector3.Lerp(transform.position, b, t); // Move objectToMove closer to b
                yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
            }
            transform.position = b;
        }
        public bool IsInPosiiton()
        {
            return transform.position == _targetPosition;
        }
        public void SetStartPosition (Vector3 startPosition)
        {
            _startPosition = startPosition;
        }

    }
}