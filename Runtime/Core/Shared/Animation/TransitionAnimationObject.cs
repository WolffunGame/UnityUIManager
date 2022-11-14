using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnityScreenNavigator.Runtime.Core.Shared
{
    /// <summary>
    ///     Base class for transition animation with ScriptableObject.
    /// </summary>
    public abstract class TransitionAnimationObject : ScriptableObject, ITransitionAnimation
    {
        public RectTransform RectTransform { get; private set; }
        public RectTransform PartnerRectTransform { get; private set; }
#if UI_ANIMATION_TIMELINE_SUPPORT
        public abstract float Duration { get; }
        public abstract bool IsCompleted { get; }
#endif
        public abstract UniTask Play(CancellationToken cancellationToken);

        void ITransitionAnimation.SetPartner(RectTransform partnerRectTransform)
        {
            PartnerRectTransform = partnerRectTransform;
        }

        void ITransitionAnimation.Setup(RectTransform rectTransform)
        {
            RectTransform = rectTransform;
            Setup();
#if UI_ANIMATION_TIMELINE_SUPPORT
            SetTime(0.0f);
#endif
        }
#if UI_ANIMATION_TIMELINE_SUPPORT
        public abstract void SetTime(float time);
#endif

        public abstract void Setup();
    }
}