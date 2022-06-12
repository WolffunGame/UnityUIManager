using Cysharp.Threading.Tasks;

namespace UnityScreenNavigator.Runtime.Core.Modal
{
    public interface IModalLifecycleEvent
    {
        /// <summary>
        /// Call this method after the modal is loaded.
        /// </summary>
        /// <returns></returns>
        UniTask Initialize();

        /// <summary>
        /// Called just before this modal is displayed by the Push transition.
        /// </summary>
        /// <returns></returns>
        UniTask WillPushEnter();

        /// <summary>
        /// Called just after this modal is displayed by the Push transition.
        /// </summary>
        void DidPushEnter();
        /// <summary>
        /// Called just before this modal is hidden by the Push transition.
        /// </summary>
        /// <returns></returns>
        UniTask WillPushExit();

        /// <summary>
        /// Called just after this modal is hidden by the Push transition.
        /// </summary>
        void DidPushExit();
        /// <summary>
        /// Called just before this modal is displayed by the Pop transition.
        /// </summary>
        /// <returns></returns>
        UniTask WillPopEnter();

        /// <summary>
        /// Called just after this modal is displayed by the Pop transition.
        /// </summary>
        void DidPopEnter();

        /// <summary>
        /// Called just before this modal is hidden by the Pop transition.
        /// </summary>
        /// <returns></returns>
        UniTask WillPopExit();

        /// <summary>
        /// Called just after this modal is hidden by the Pop transition.
        /// </summary>
        void DidPopExit();

        /// <summary>
        /// Called just before this modal is released.
        /// </summary>
        /// <returns></returns>
        UniTask Cleanup();
    }
}