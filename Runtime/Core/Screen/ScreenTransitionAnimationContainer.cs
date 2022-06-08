using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Serialization;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Animation;
using UnityScreenNavigator.Runtime.Foundation;
using Object = UnityEngine.Object;

namespace UnityScreenNavigator.Runtime.Core.Screen
{
    [Serializable]
    public sealed class ScreenTransitionAnimationContainer
    {
        [SerializeField] private List<TransitionAnimation> _pushEnterAnimations = new List<TransitionAnimation>();
        [SerializeField] private List<TransitionAnimation> _pushExitAnimations = new List<TransitionAnimation>();
        [SerializeField] private List<TransitionAnimation> _popEnterAnimations = new List<TransitionAnimation>();
        [SerializeField] private List<TransitionAnimation> _popExitAnimations = new List<TransitionAnimation>();

        public List<TransitionAnimation> PushEnterAnimations => _pushEnterAnimations;
        public List<TransitionAnimation> PushExitAnimations => _pushExitAnimations;
        public List<TransitionAnimation> PopEnterAnimations => _popEnterAnimations;
        public List<TransitionAnimation> PopExitAnimations => _popExitAnimations;

        public ITransitionAnimation GetAnimation(bool push, bool enter, string partnerTransitionIdentifier)
        {
            var anims = GetAnimations(push, enter);
            var anim = anims.FirstOrDefault(x => x.IsValid(partnerTransitionIdentifier));
            var result = anim?.GetAnimation();
            return result;
        }

        private IReadOnlyList<TransitionAnimation> GetAnimations(bool push, bool enter)
        {
            if (push)
            {
                return enter ? _pushEnterAnimations : _pushExitAnimations;
            }

            return enter ? _popEnterAnimations : _popExitAnimations;
        }

       
    }
}