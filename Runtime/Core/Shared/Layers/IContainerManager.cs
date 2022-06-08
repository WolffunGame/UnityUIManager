using Cysharp.Threading.Tasks;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Shared.Layers
{
    /// <summary>
    /// Manage Modals and Screens
    /// </summary>
    public interface IContainerManager<TWindow> : IContainerLayer where  TWindow: Window
    {
        /// <summary>
        /// Pop a window.
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <returns></returns>
        UniTask Pop(bool playAnimation);

        /// <summary>
        /// Push a window.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        UniTask<TWindow> Push(WindowOption option);
    }
}