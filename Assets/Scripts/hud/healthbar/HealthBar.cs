using UnityEngine;

namespace WizardMatch
{
    public class HudEnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private ScalingBar _scaleBar;
        [SerializeField] public Character character;
        [SerializeField] public GenericFader fader;

        void Awake()
        {
            if (!character || !_scaleBar)
            {
                    Debug.LogError("ERROR : Health bar does not have corresponding character! Aborting...");
                    Destroy(gameObject);
            }
            
            _scaleBar.maxValue = character.characterData.baseHP;
            _scaleBar.value = character.characterData.baseHP;

            _scaleBar.barColor = Color.green;
            _scaleBar.backColor = Color.red;
        }
        void Update()
        {
            MonitorState();
        }       
        void MonitorState()
        {
            _scaleBar.value = character.hp;
            if (_scaleBar.value <= 0)
                fader.StartFade();
        }
    }
}