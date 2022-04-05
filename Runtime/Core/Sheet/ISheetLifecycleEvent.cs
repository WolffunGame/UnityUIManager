#if USN_USE_ASYNC_METHODS
using Cysharp.Threading.Tasks;
#else
using System.Collections;
#endif

namespace UnityScreenNavigator.Runtime.Core.Sheet
{
    public interface ISheetLifecycleEvent
    {
#if USN_USE_ASYNC_METHODS
        UniTask Initialize();
#else
        IEnumerator Initialize();
#endif

#if USN_USE_ASYNC_METHODS
        UniTask WillEnter();
#else
        IEnumerator WillEnter();
#endif
        void DidEnter();

#if USN_USE_ASYNC_METHODS
        UniTask WillExit();
#else
        IEnumerator WillExit();
#endif

        void DidExit();

#if USN_USE_ASYNC_METHODS
        UniTask Cleanup();
#else
        IEnumerator Cleanup();
#endif
    }
}