/// <summary>
/// Dead simple interface for playing animations. 
/// Made as a safety catch for anything else in the future we'd like to animate.
/// </summary>
public interface IAnimatable
{
    public void PlayAnimation(string animation);
    public void OnAnimationFinish(string animation);
    public void OnAnimationBegin(string animation);
    
}