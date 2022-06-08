using System;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Animation;
using UnityScreenNavigator.Runtime.Foundation;
using Object = UnityEngine.Object;

namespace UnityScreenNavigator.Runtime.Core.Modal
{
    [Serializable]
    public sealed class ModalBackdropTransitionAnimationContainer
    {
        [SerializeField] private TransitionAnimation _enterAnimation;
        [SerializeField] private TransitionAnimation _exitAnimation;

        public TransitionAnimation EnterAnimation => _enterAnimation;
        public TransitionAnimation ExitAnimation => _exitAnimation;

        public ITransitionAnimation GetAnimation(bool enter)
        {
            var transitionAnimation = enter ? _enterAnimation : _exitAnimation;
            return transitionAnimation.GetAnimation();
        }

    }
}