using Cysharp.Threading.Tasks;

namespace UnityScreenNavigator.Runtime.Foundation.Animation
{
    public interface IAnimation
    {
        /// <summary>
        /// duration of animation
        /// </summary>
        float Duration { get; }
        /// <summary>
        /// Use to play animation over time, such as for Timeline or custom animation.
        /// </summary>
        /// <param name="time"></param>
        void SetTime(float time);
        /// <summary>
        /// Entry point to start animation. Work with animation system such as DOTween of other system does not need update every time.
        /// </summary>
        /// <returns></returns>
        UniTask Play();
    }
}