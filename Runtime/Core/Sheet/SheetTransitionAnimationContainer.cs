using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Animation;
using UnityScreenNavigator.Runtime.Foundation;
using Object = UnityEngine.Object;

namespace UnityScreenNavigator.Runtime.Core.Sheet
{
    [Serializable]
    public class SheetTransitionAnimationContainer
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