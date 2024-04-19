using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WizardMatch;
/// <summary>
/// Generic sprite fader. Fades from solid to completely invisible over the course of a few seconds. 
/// </summary>
public class GenericFader : MonoBehaviour
{
    public float seconds = 1.0f;
    public Timer fadeTimer;
    
    [SerializeField] private bool fade = false;

    [SerializeField] private bool fadeIn = false;

    [SerializeField] private List<SpriteRenderer> _sprites;
    [SerializeField] private List<TextMeshPro> _texts;


    void Awake()
    {
        fadeTimer = new Timer(seconds);
        if (_sprites.Count == 0)
        {
            Debug.LogError("ERROR : No sprite renderer associated with " + gameObject.name + "! Aborting");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (fade)
        {
            Fade();
            fadeTimer.Tick(Time.deltaTime);
        }
    }
    public void StartFade()
    {
        fade = true;
    }
    void Fade()
    {
        float t = fadeTimer.RemaingSeconds / fadeTimer.MaxDuration;
        foreach(SpriteRenderer s in _sprites)
        {
            Color c = s.color;
            if (fadeIn)
                s.color = new Color (c.r,c.g,c.b, 1.0f - t);
            else
                s.color = new Color(c.r,c.g,c.b,t);
        }
        foreach(TextMeshPro te in _texts)
        {
            Color c = te.color;
            if (fadeIn)
                te.color = new Color (c.r,c.g,c.b, 1.0f - t);
            else
                te.color = new Color(c.r,c.g,c.b,t);
        }

    }
    public void ResetFader(float value = 1.0f)
    {
        fadeTimer.SetTimer(seconds);
    }
}
