#if UI_ANIMATION_TIMELINE_SUPPORT
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityScreenNavigator.Runtime.Foundation.Animation;

namespace UnityScreenNavigator.Tests.PlayMode.Foundation
{
    public class FakeAnimation : IAnimation
    {
        public float Progress { get; private set; }
        public float Duration { get; }

        public FakeAnimation(float duration = 1.0f)
        {
            Duration = duration;
        }
        
        public void SetTime(float time)
        {
            time = Math.Min(Duration, time);
            Progress = time / Duration;
        }

        public UniTask Play(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
#endif