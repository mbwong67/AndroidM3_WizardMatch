using UnityEngine;

namespace WizardMatch
{
    public class HudEnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 10.0f;

        [SerializeField] private int _hpValue;
        [SerializeField] public int maxHP;
        [SerializeField] private Transform _spriteMask;
        [SerializeField] private Vector3 _offset;
        [SerializeField] private Vector3 _spriteMaskTargetOffset;
        [SerializeField] public Character character;

        void Awake()
        {
            if (!character || !_spriteMask)
            {
                    Debug.LogError("ERROR : Health bar does not have corresponding character! Aborting...");
                    Destroy(gameObject);
            }
            maxHP = character.characterData.baseHP;
            _hpValue = character.hp;
        }
        void Update()
        {
            MonitorState();
        }       
        void MonitorState()
        {
            _hpValue = character.hp;

            Vector3 curPos = character.transform.position + _offset;
            Vector3 curTarget = _spriteMaskTargetOffset + curPos;
            transform.position = curPos;

            float ratio = (float) _hpValue / (float) maxHP;
            curTarget = Vector3.Lerp(curPos,curTarget, 1.0f - ratio);
            _spriteMask.position = Vector3.Lerp(_spriteMask.position,curTarget,_moveSpeed * Time.deltaTime);
        }
    }
}