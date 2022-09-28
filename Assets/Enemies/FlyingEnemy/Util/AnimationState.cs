/// <summary>
/// Author: Rayshawn Eatmon
/// Date: July 2022
/// </summary>
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationState
{
    private List<FlyingEnemyState> BirdStates = new List<FlyingEnemyState>();
    private List<FlyingEnemyState> BeeStates = new List<FlyingEnemyState>();
    private bool IsBirdAnimation;
    public AnimationState(Animator animator, bool isBirdAnimation)
    {
        IsBirdAnimation = isBirdAnimation;
        if(isBirdAnimation)
        {
            BirdStates.Add(new FlyingEnemyState(animator, BirdAnimationTransition.Idle));
            BirdStates.Add(new FlyingEnemyState(animator, BirdAnimationTransition.Patrol));
            BirdStates.Add(new FlyingEnemyState(animator, BirdAnimationTransition.Approach));
            BirdStates.Add(new FlyingEnemyState(animator, BirdAnimationTransition.SwoopAttack));
            BirdStates.Add(new FlyingEnemyState(animator, BirdAnimationTransition.Rest));
            BirdStates.Add(new FlyingEnemyState(animator, BirdAnimationTransition.Hurt));
            BirdStates.Add(new FlyingEnemyState(animator, BirdAnimationTransition.Death));
            BirdStates.Add(new FlyingEnemyState(animator, BirdAnimationTransition.Leave));
        }
        else
        {
            BeeStates.Add(new FlyingEnemyState(animator, BeeAnimationTransition.Idle));
            BeeStates.Add(new FlyingEnemyState(animator, BeeAnimationTransition.Patrol));
            BeeStates.Add(new FlyingEnemyState(animator, BeeAnimationTransition.Approach));
            BeeStates.Add(new FlyingEnemyState(animator, BeeAnimationTransition.Attack));
            BeeStates.Add(new FlyingEnemyState(animator, BeeAnimationTransition.Reset));
            BeeStates.Add(new FlyingEnemyState(animator, BeeAnimationTransition.Leave));
            BeeStates.Add(new FlyingEnemyState(animator, BeeAnimationActions.Death));
        }
    }


    public void TurnOnState(string state)
    {
        if(IsBirdAnimation)
        {
            BirdStates.ForEach(b =>
            {
                if (b.Name == state)
                    b.SetAnimationState(state, true);
                else
                    b.SetAnimationState(b.Name, false);
            });
        }
        else
        {
            BeeStates.ForEach(b =>
            {
                if (b.Name == state)
                    b.SetAnimationState(state, true);
                else
                    b.SetAnimationState(b.Name, false);
            });
        }
    }

    public void TurnOffAllStates()
    {
        if (IsBirdAnimation)
        {
            BirdStates.ForEach(b =>
            {
                b.SetAnimationState(b.Name, false);
            });
        }
        else
        {
            BeeStates.ForEach(b =>
            {
                b.SetAnimationState(b.Name, false);
            });
        }       
    }

    public void PlayAnimation(string state)
    {
        if (IsBirdAnimation)
        {
            BirdStates?.Where(b => b.Name == state)?.Single().PlayAnimation(state);
        }
        else
        {
            BeeStates?.Where(b => b.Name == state)?.Single().PlayAnimation(state);
        }
    }
}
