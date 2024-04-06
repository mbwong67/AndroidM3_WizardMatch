using System.Collections.Generic;
using UnityEngine;
using WizardMatch;
/// <summary>
/// Generic sprite fader. Fades from solid to completely invisible over the course of a few seconds. 
/// </summary>
public class GenericFader : MonoBehaviour
{
    public float seconds = 1.0f;
    public Timer fadeTimer;
    bool fade = false;
    [SerializeField] List<SpriteRenderer> _sprites;

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
            s.color = new Color(c.r,c.g,c.b,t);
        }
    }
    public void ResetFader(float value = 1.0f)
    {
        fadeTimer.SetTimer(seconds);
    }
}
