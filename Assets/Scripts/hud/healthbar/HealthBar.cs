using UnityEngine;

namespace WizardMatch
{
    public class HudEnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 10.0f;

        [SerializeField] private int _hpValue;
        [SerializeField] public int maxHP;
        [SerializeField] private Transform _barFill;
        [SerializeField] private Vector3 _offset;
        [SerializeField] private Vector3 _spriteMaskTargetOffset;
        [SerializeField] private Vector3 _initialScale;
        [SerializeField] public Character character;
        [SerializeField] public GenericFader fader;

        void Awake()
        {
            if (!character || !_barFill)
            {
                    Debug.LogError("ERROR : Health bar does not have corresponding character! Aborting...");
                    Destroy(gameObject);
            }
            maxHP = character.characterData.baseHP;
            _hpValue = character.hp;
            _spriteMaskTargetOffset.x *= (1 - transform.localScale.x);
            _spriteMaskTargetOffset.y *= (1 - transform.localScale.y);

            _initialScale = new Vector3
                (transform.localScale.x,_barFill.transform.localScale.y,_barFill.transform.localScale.z);
        }
        void Update()
        {
            MonitorState();
        }       
        void MonitorState()
        {
            _hpValue = character.hp;
            if (_hpValue <= 0)
                fader.StartFade();

            


            float ratio = (float) _hpValue / (float) maxHP;
            float targetX = -0.5f * _initialScale.x + (ratio * 0.5f * _initialScale.x);
            // float targetX = -_initialScale.x + (ratio * 0.5f * _initialScale.x);
            
            Vector3 curPos = character.transform.position + _offset;
            transform.position = curPos;
            Vector3 curTarget = _spriteMaskTargetOffset / 2f  + new Vector3(targetX,0,0) + curPos;

            Vector3 curTargetScale = new Vector3(ratio, _initialScale.y, _initialScale.z);
            _barFill.transform.localScale = Vector3.Lerp(_barFill.transform.localScale,curTargetScale,_moveSpeed * Time.deltaTime);
            _barFill.position = Vector3.Lerp(_barFill.position,curTarget,_moveSpeed * Time.deltaTime);
        }
    }
}