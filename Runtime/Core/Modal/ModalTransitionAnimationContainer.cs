using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Animation;

namespace UnityScreenNavigator.Runtime.Core.Modal
{
    [Serializable]
    public class ModalTransitionAnimationContainer
    {
        [SerializeField] private List<TransitionAnimation> _enterAnimations = new List<TransitionAnimation>();
        [SerializeField] private List<TransitionAnimation> _exitAnimations = new List<TransitionAnimation>();

        public List<TransitionAnimation> EnterAnimations => _enterAnimations;
        public List<TransitionAnimation> ExitAnimations => _exitAnimations;

        public ITransitionAnimation GetAnimation(bool enter, string partnerTransitionIdentifier)
        {
            var anims = enter ? _enterAnimations : _exitAnimations;
            var anim = anims.FirstOrDefault(x => x.IsValid(partnerTransitionIdentifier));
            var result = anim?.GetAnimation();
            return result;
        }
    }
}