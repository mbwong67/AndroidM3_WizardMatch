using UnityEngine;

namespace WizardMatch
{
    public class ScalingBar : MonoBehaviour
    {
        [SerializeField] public float value = 0.0f;
        [SerializeField] public float maxValue = 0.0f;
        [SerializeField] public Color barColor;
        [SerializeField] public Color defaultBarColor;
        [SerializeField] public Color backColor;

        
        [SerializeField] private float _moveSpeed = 10.0f;
        [SerializeField] private SpriteRenderer _barFill;
        [SerializeField] private SpriteRenderer _backFill;
        [SerializeField] private Vector3 _offset;
        [SerializeField] private Vector3 _spriteMaskTargetOffset;
        [SerializeField] private Vector3 _initialScale;

        void Awake()
        {
            value = maxValue;
            _spriteMaskTargetOffset.x *= 1 - transform.localScale.x;
            _spriteMaskTargetOffset.y *= 1 - transform.localScale.y;

            _initialScale = new Vector3
                (transform.localScale.x,_barFill.transform.localScale.y,_barFill.transform.localScale.z);
            value = maxValue;
            _backFill.color = backColor;
        }
        void Update()
        {
            MonitorState();
        }       
        void MonitorState()
        {
            _barFill.color = barColor;
            if (value > maxValue)
                value = maxValue;
            if (value <= 0)
                value = 0;
            
            float ratio = (float) value / (float) maxValue;
            float targetX = -0.5f * _initialScale.x + (ratio * 0.5f * _initialScale.x);
            
            Vector3 curPos = _offset;

            curPos += transform.position;

            transform.position = curPos;
            Vector3 curTarget = _spriteMaskTargetOffset / 2f  + new Vector3(targetX,0,0) + curPos;

            Vector3 curTargetScale = new Vector3(ratio, _initialScale.y, _initialScale.z);
            _barFill.transform.localScale = Vector3.Lerp(_barFill.transform.localScale,curTargetScale,_moveSpeed * Time.deltaTime);
            _barFill.transform.position = Vector3.Lerp(_barFill.transform.position,curTarget,_moveSpeed * Time.deltaTime);
        }

    }
}