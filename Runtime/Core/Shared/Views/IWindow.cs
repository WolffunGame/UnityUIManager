using UnityScreenNavigator.Runtime.Foundation.Coroutine;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    /// <summary>
    /// A window on the screen.
    /// Handles the view lifecycle.
    /// </summary>
    public interface IWindow
    {
        /// <summary>
        /// The name of the window.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Returns  "true" if this window created.
        /// </summary>
        bool Created { get; }

        /// <summary>
        /// Returns  "true" if this window dismissed.
        /// </summary>
        bool Dismissed { get; }

        /// <summary>
        /// Returns  "true" if this window visibility.
        /// </summary>
        bool Visibility { get; }

        /// <summary>
        /// Returns  "true" if this window activated.
        /// </summary>
        bool Activated { get; }

        /// <summary>
        /// The WindowManager of the window.
        /// </summary>
        IWindowManager WindowManager { get; set; }
        
        //TODO Create a WindowType enum?
        //for example: 

        /// <summary>
        /// The priority of the window.When pop-up windows are queued to open, 
        /// windows with higher priority will be opened first.
        /// </summary>
        int WindowPriority { get; set; }

        /// <summary>
        /// Create window
        /// </summary>
        /// <param name="bundle"></param>
        void Create(IBundle bundle = null);

        // /// <summary>
        // /// 
        // /// </summary>
        // /// <exception cref="System.InvalidOperationException"></exception>
        // AsyncProcessHandle Show(ShowWindowOption option);
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // /// <exception cref="System.InvalidOperationException"></exception>
        // AsyncProcessHandle Hide(bool playAnimation = true);
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // AsyncProcessHandle Dismiss(bool playAnimation = true);

    }
}