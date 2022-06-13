#if UI_ANIMATION_TIMELINE_SUPPORT
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityScreenNavigator.Runtime.Core.Shared
{
    public sealed class TimelineTransitionAnimationBehaviour : TransitionAnimationBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private TimelineAsset _timelineAsset;
        
        public override float Duration => (float)_timelineAsset.duration;
        public override bool IsCompleted => _director.state == PlayState.Paused;

        public override void Setup()
        {
            _director.playableAsset = _timelineAsset;
            _director.time = 0;
            _director.initialTime = 0;
            _director.playOnAwake = false;
            _director.timeUpdateMode = DirectorUpdateMode.GameTime;
            _director.extrapolationMode = DirectorWrapMode.None;
        }

        public override UniTask Play()
        {
            return UniTask.CompletedTask;
        }

        public override void SetTime(float time)
        {
            _director.time = time;
            _director.Evaluate();
        }
        
    }
}
#endif