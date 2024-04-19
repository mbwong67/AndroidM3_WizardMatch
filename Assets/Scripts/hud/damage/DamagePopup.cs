using TMPro;
using UnityEngine;

namespace WizardMatch
{
    public class DamagePopup : MonoBehaviour
    {
        public int damageToDisplay;

        [SerializeField] private TextMeshPro _numberText;
        [SerializeField] private GenericFader _fader;

        [SerializeField][Range(0.1f,20.0f)] private float _gravity = 9.81f;
        [SerializeField][Range(0.1f,20.0f)] private float _moveSpeed = 1.0f;
        [SerializeField] private Vector2 _launchAngle;
        [SerializeField] private Vector2 _gravityVector;
        
        private Timer _startTimer = new Timer(1.0f);

        void Awake()
        {
            _startTimer.OnTimerEnd += FadeAway;

            float angleRange = Random.Range(60.0f,120.0f);
            angleRange = Mathf.Deg2Rad * angleRange;
            float xComp = Mathf.Cos(angleRange);
            float yComp = Mathf.Sin(angleRange);

            _launchAngle = new Vector2(xComp, yComp);
            _gravityVector = Vector2.down * _gravity * Time.deltaTime;
        }

        void Update()
        {
            _startTimer.Tick(Time.deltaTime);
            Move();
        }
        void Move()
        {
            Vector3 curPos = transform.position;
            curPos += (Vector3) _launchAngle * _moveSpeed * Time.deltaTime + (Vector3) _gravityVector * Time.deltaTime;
            _gravityVector += _gravityVector * Time.deltaTime;
            transform.position = curPos;
        }

        public void SetDamageNumber(int number)
        {
            _numberText.text = number.ToString();
        }
        void FadeAway()
        {
            _fader.StartFade();
            _startTimer.OnTimerEnd -= FadeAway;
            _startTimer.SetTimer(1.0f);
            _startTimer.OnTimerEnd += DestroyObject;
        }
        void DestroyObject()
        {
            _startTimer.OnTimerEnd -= DestroyObject;
            Destroy(gameObject);
        }
    }
}