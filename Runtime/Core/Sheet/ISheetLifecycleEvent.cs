using Cysharp.Threading.Tasks;

namespace UnityScreenNavigator.Runtime.Core.Sheet
{
    public interface ISheetLifecycleEvent
    {
        UniTask Initialize();

        UniTask WillEnter();
        void DidEnter();

        UniTask WillExit();

        void DidExit();

        UniTask Cleanup();
    }
}