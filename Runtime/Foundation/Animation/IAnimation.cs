using Cysharp.Threading.Tasks;

namespace UnityScreenNavigator.Runtime.Foundation.Animation
{
    public interface IAnimation
    {
#if UI_ANIMATION_TIMELINE_SUPPORT
        /// <summary>
        /// duration of animation
        /// </summary>
        float Duration { get; }

        /// <summary>
        /// Use to play animation over time, such as for Timeline or custom animation.
        /// </summary>
        /// <param name="time"></param>
        void SetTime(float time);
#endif
        /// <summary>
        /// Entry point to start animation. Work with animation system such as DOTween of other system does not need update every time.
        /// </summary>
        /// <returns></returns>
        UniTask Play();
    }
}