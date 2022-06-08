using UnityScreenNavigator.Runtime.Foundation.Animation;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    /// <summary>
    /// The base class for small views that are not <see cref="Window"/>.
    /// They have own animation that is not handled by the <see cref="UnityScreenNavigator.Runtime.Core.Shared.Layers.IContainerLayer"/>
    /// </summary>
    public class UIElement : UIView
    {
        private IAnimation _enterAnimation;
        private IAnimation _exitAnimation;
        
        public virtual IAnimation EnterAnimation
        {
            get { return this._enterAnimation; }
            set { this._enterAnimation = value; }
        }

        public virtual IAnimation ExitAnimation
        {
            get { return this._exitAnimation; }
            set { this._exitAnimation = value; }
        }
    }
}