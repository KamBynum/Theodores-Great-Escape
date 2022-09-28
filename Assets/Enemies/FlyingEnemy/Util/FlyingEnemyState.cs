/// <summary>
/// Author: Rayshawn Eatmon
/// Date: July 2022
/// </summary>
using UnityEngine;

public class FlyingEnemyState
{
    private Animator Animator { get; set; }
    public string Name { get; }
    public FlyingEnemyState(Animator anim, string animationName)
    {
        Name = animationName;
        Animator = anim;
    }

    public void SetAnimationState(string name, bool isOn)
    {
        if(name == Name)
        {
            if (name != "Death")
            {
                Animator.SetBool(Name, isOn);
            }
        }
    }
    public void PlayAnimation(string animationName)
    {
        Animator.Play(animationName);
    }
}
