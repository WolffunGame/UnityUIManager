using System.Collections;
#if USN_USE_ASYNC_METHODS
using Cysharp.Threading.Tasks;
#endif

namespace UnityScreenNavigator.Runtime.Core.Modal
{
    public interface IModalLifecycleEvent
    {
        /// <summary>
        /// Call this method after the modal is loaded.
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask Initialize();
#else
        IEnumerator Initialize();
#endif
        /// <summary>
        /// Called just before this modal is displayed by the Push transition.
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask WillPushEnter();
#else
        IEnumerator WillPushEnter();
#endif
        /// <summary>
        /// Called just after this modal is displayed by the Push transition.
        /// </summary>
        void DidPushEnter();
        /// <summary>
        /// Called just before this modal is hidden by the Push transition.
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask WillPushExit();
#else
        IEnumerator WillPushExit();
#endif
        /// <summary>
        /// Called just after this modal is hidden by the Push transition.
        /// </summary>
        void DidPushExit();
        /// <summary>
        /// Called just before this modal is displayed by the Pop transition.
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask WillPopEnter();
#else
        IEnumerator WillPopEnter();
#endif
        /// <summary>
        /// Called just after this modal is displayed by the Pop transition.
        /// </summary>
        void DidPopEnter();

        /// <summary>
        /// Called just before this modal is hidden by the Pop transition.
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask WillPopExit();
#else
        IEnumerator WillPopExit();
#endif
        /// <summary>
        /// Called just after this modal is hidden by the Pop transition.
        /// </summary>
        void DidPopExit();

        /// <summary>
        /// Called just before this modal is released.
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask Cleanup();
#else
        IEnumerator Cleanup();
#endif
    }
}