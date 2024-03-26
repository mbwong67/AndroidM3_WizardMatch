using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WizardMatch 
{
    public class proto_mainMenu : MonoBehaviour
    {
        [SerializeField] private BlackScreenFader _fader;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;

        void Awake()
        {
            _playButton.interactable = false;
            _quitButton.interactable = false;
            
            _playButton.onClick.AddListener(OnStart);
            _quitButton.onClick.AddListener(OnQuit);

            _fader.OnFadeIn += Initialize;
        }

        void Initialize()
        {
            _playButton.interactable = true;
            _quitButton.interactable = true;
        }
        void GreyOutButtons()
        {
            _playButton.interactable = false;
            _quitButton.interactable = false;
        }
        void OnStart()
        {
            GreyOutButtons();
            _fader.OnFadeOut += StartGame;
            _fader.PlayAnimation("FadeOut");
        }
        void OnQuit()
        {
            GreyOutButtons();
            _fader.OnFadeOut += QuitGame;
            _fader.PlayAnimation("FadeOut");
        }

        void StartGame()
        {
            _fader.OnFadeOut -= StartGame;
            SceneManager.LoadScene("matcher");
        }
        void QuitGame()
        {
            _fader.OnFadeOut -= QuitGame;
            Application.Quit(0);
        }
    }
}