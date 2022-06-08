using System;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Animation;

namespace UnityScreenNavigator.Runtime.Interactivity.Animation
{
    [Serializable]
    public sealed class InteractivityTransitionAnimationContainer
    {
        [SerializeField] private TransitionAnimation enterTransitionAnimation;
        [SerializeField] private TransitionAnimation exitTransitionAnimation;
        
        
        public ITransitionAnimation GetAnimation(bool enter)
        {
            var anim = enter ? enterTransitionAnimation : exitTransitionAnimation;
            return anim.GetAnimation();
        }
    }
}